using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Уведомление о приёмке прихода
    /// </summary>
    public class InboundNotification : Message
    {
        /// <summary>
        /// Приход
        /// </summary>
        public Inbound Inbound { get; set; }
        /// <summary>
        /// Номер запроса на приход
        /// </summary>
        public string InstructionNumber { get; set; }
        /// <summary>
        /// Статус (состояние)
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Продавец складских и логистических услуг
        /// </summary>
        public LegalEntity ServiceSeller { get; private set; }
        /// <summary>
        /// Покупатель складских и логистических услуг
        /// </summary>
        public LegalEntity ServiceBuyer { get; private set; }
        /// <summary>
        /// Отправитель груза
        /// </summary>
        public LegalEntity Shipper { get; private set; }
        /// <summary>
        /// Получатель груза
        /// </summary>
        public LegalEntity Receiver { get; private set; }
        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public InboundNotification(long id, string number, byte version, DateTime date, Inbound inbound,
            LegalEntity sender, LegalEntity recipient,
            LegalEntity serviceBuyer, LegalEntity serviceSeller, LegalEntity shipper, LegalEntity receiver,
            string note = "")
            : base(id, number, 'O', "INBOUND_NOTIFICATION", version, date, sender, recipient, note)
        {
            Inbound = inbound ?? throw new ArgumentNullException(nameof(inbound));
            ServiceSeller = serviceSeller ?? throw new ArgumentNullException(nameof(serviceSeller));
            ServiceBuyer = serviceBuyer ?? throw new ArgumentNullException(nameof(serviceBuyer));
            Shipper = shipper ?? throw new ArgumentNullException(nameof(shipper));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }
    }
}
