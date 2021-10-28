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
        public long Id { get; private set; }
        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; private set; }
        /// <summary>
        /// Вид
        /// </summary>
        public char Kind { get; private set; }
        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Версия
        /// </summary>
        public byte Version { get; private set; }
        /// <summary>
        /// Дата (создания)
        /// </summary>
        public DateTime Date { get; private set; }
        /// <summary>
        /// Отправитель
        /// </summary>
        public LegalEntity Sender { get; private set; }
        /// <summary>
        /// Получатель
        /// </summary>
        public LegalEntity Recipient { get; private set; }
        /// <summary>
        /// Тело
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public string Note { get; private set; }
        public Message(long id, string number, char kind, string type, byte version, DateTime date, 
            LegalEntity sender, LegalEntity recipient, string note = null)
        {
            Id = id > 0 ? id : -1;
            Number = !string.IsNullOrWhiteSpace(number) ? number.Trim() : string.Empty;
            Kind = char.ToUpper(kind);
            Type = !string.IsNullOrWhiteSpace(type) ? type.Trim() : string.Empty;            
            Version = version > 0 ? version : Convert.ToByte(1);
            Date = date >= DateTime.Now ? date : DateTime.Now;
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
            Body = string.Empty; 
            Note = !string.IsNullOrWhiteSpace(note) ? note.Trim() : string.Empty;
        }
    }
}
