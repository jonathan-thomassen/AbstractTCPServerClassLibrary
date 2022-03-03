using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AbstractTCPServerClassLibrary {
    internal class JsonTraceListener : TraceListener {
        private string _traceString;
        private string _filePath;

        public JsonTraceListener(string path) {
            _filePath = path;
            _traceString = "{\n\"traceLog\":\n[\n";            
        }

        public override void Write(string? message) {
            _traceString += JsonSerializer.Serialize(message) + ":";
            File.WriteAllText(_filePath, _traceString + "");
        }

        public override void WriteLine(string? message) {
            _traceString += JsonSerializer.Serialize(message) + ",\n";
            File.WriteAllText(_filePath, _traceString.Substring(0, _traceString.Length-2) + "\n]\n}");
        }
    }
}
