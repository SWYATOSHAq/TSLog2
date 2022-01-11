using System;
using System.IO;

namespace TSLog.Log
{
    /// <summary>
    /// Класс для записи журнала сообщений (например, об ошибках) в текстовый файл
    /// </summary>
    public class LogWriter : IDisposable
    {
        private readonly StreamWriter _stream = null;

        public LogWriter(string fileName)
        {
            _stream = new StreamWriter(fileName);
        }

        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
            GC.SuppressFinalize(this);
        }

        public void WriteLog(string message)
        {
            _stream.WriteLine(message);
        }
    }
}