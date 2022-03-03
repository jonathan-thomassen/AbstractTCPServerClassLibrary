using System.Diagnostics;
using System.Net.Sockets;

namespace AbstractTCPServerClassLibrary.TCPServer {
    /// <summary>
    /// Abstract TCP Server class to be implemented by an inherited class.
    /// </summary>
    public abstract class AbstractTCPServer {
        private int _port;
        private int _shutDownPort;
        private string _name;
        private bool _running;

        private TraceSource ts = new TraceSource("TCP Server Framework");

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Port #</param>
        /// <param name="name">Name of server</param>
        protected AbstractTCPServer(int port, string name, int shutDownPort, string debugLevel) {
            ts.Switch = new SourceSwitch("TCP Server Log", debugLevel);

            TraceListener consoleLog = new ConsoleTraceListener();
            ts.Listeners.Add(consoleLog);

            TraceListener fileLog = new TextWriterTraceListener(new StreamWriter("TraceTCPServer.txt"));
            fileLog.Filter = new EventTypeFilter(SourceLevels.Warning);
            ts.Listeners.Add(fileLog);

            TraceListener xmlLog = new XmlWriterTraceListener(new StreamWriter("TraceTCPServer.xml"));
            xmlLog.Filter = new EventTypeFilter(SourceLevels.Warning);
            ts.Listeners.Add(xmlLog);

            TraceListener eventLog = new EventLogTraceListener("Application");
            ts.Listeners.Add(eventLog);

            TraceListener jsonLog = new JsonTraceListener("TraceTCPServer.json");
            ts.Listeners.Add(jsonLog);

            _port = port;
            _shutDownPort = shutDownPort;
            _name = name;
            _running = true;

            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, _port);
            TcpListener stopListener = new TcpListener(System.Net.IPAddress.Any, _shutDownPort);

            listener.Start();
            stopListener.Start();

            //Console.WriteLine($"Server {_name} started.");
            ts.TraceEvent(TraceEventType.Information, 256, $"Server {_name} started.");
            ts.Flush();

            while (_running) {
                Task.Run(() => NewClient(listener));
                Task.Run(() => StopClient(stopListener));
            }
        }

        private void NewClient(TcpListener listener) {
            TcpClient socket = listener.AcceptTcpClient();

            //Console.WriteLine("New client: " + socket.Client.RemoteEndPoint);
            ts.TraceEvent(TraceEventType.Information, 256, "New client: " + socket.Client.RemoteEndPoint);
            ts.Flush();

            Task.Run(() => {
                NetworkStream ns = socket.GetStream();
                StreamReader reader = new StreamReader(ns);
                StreamWriter writer = new StreamWriter(ns);

                TcpServerWork(reader, writer);

                //Console.WriteLine("Closing connection: " + socket.Client.RemoteEndPoint);
                ts.TraceEvent(TraceEventType.Information, 256, "Closing connection: " + socket.Client.RemoteEndPoint);
                ts.Flush();

                socket.Close();
            });
        }

        private void StopClient(TcpListener listener) {
            if (listener.Pending()) {
                TcpClient socket = listener.AcceptTcpClient();

                //Console.WriteLine($"Closing server {_name}");
                ts.TraceEvent(TraceEventType.Warning, 256, $"Closing server {_name}");
                ts.Close();

                SetRunningFalse();
            } else {
                Thread.Sleep(2 * 1000);
            }
        }

        private void SetRunningFalse() {
            _running = false;
        }

        /// <summary>
        /// Abstract method which takes a StreamReader and a StreamWriter and does the required work for the server.
        /// </summary>
        /// <param name="reader">A StreamReader object</param>
        /// <param name="writer">A StreamWriter object</param>
        public abstract void TcpServerWork(StreamReader reader, StreamWriter writer);
    }
}