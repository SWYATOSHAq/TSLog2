using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Базовый класс для сообщений
    /// </summary>
    public class Message
    {
        public long Id { get; set; }
        public string Number { get; set; }
        public char Kind { get; set; }
        public string Type { get; set; }
        public byte Version { get; set; }
        public DateTime Date { get; set; }
        public LegalEntity Sender { get; set; }
        public LegalEntity Recipient { get; set; }
        public string Body { get; set; }
        public Message(long id, char kind, string type, byte version, string body = null)
        {
            Id = id > 0 ? id : -1;
            Kind = char.ToUpper(kind);
            Type = !string.IsNullOrWhiteSpace(type) ? type.Trim() : string.Empty;
            Version = version > 0 ? version : Convert.ToByte(1);
            Body = body;
        }
    }
}
