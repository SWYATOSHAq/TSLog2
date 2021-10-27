using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Базовый класс для сообщений
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Вид
        /// </summary>
        public char Kind { get; set; }
        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Версия
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// Дата (создания)
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Отправитель
        /// </summary>
        public LegalEntity Sender { get; set; }
        /// <summary>
        /// Получатель
        /// </summary>
        public LegalEntity Recipient { get; set; }
        /// <summary>
        /// Тело
        /// </summary>
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
