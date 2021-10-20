using System;
using System.IO;

namespace TSLog.Log
{
    public class LogWriter : IDisposable
    {
        private StreamWriter _stream = null;
        public LogWriter(string fileName) 
        {
            _stream = new StreamWriter(fileName);
        }

        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
        }

        public void WriteLog(string message) 
        {
            _stream.WriteLine(message);
        }
    }
}
