using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Сообщение запрос на приход товара на склад
    /// </summary>
    public class InboundInstruction : Message
    {
        /// <summary>
        /// Приход товара на склад
        /// </summary>
        public Inbound Inbound { get; private set; }
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
        public InboundInstruction(long id, string number, byte version, DateTime date, Inbound inbound,
            LegalEntity sender, LegalEntity recipient, 
            LegalEntity serviceSeller, LegalEntity serviceBuyer, LegalEntity shipper, LegalEntity receiver,
            string note = "")
            : base(id, number, 'I', "INBOUND_INSTRUCTION", version, date, sender, recipient, note)
        {
            Inbound = inbound ?? throw new ArgumentNullException(nameof(inbound));
            ServiceSeller = serviceSeller ?? throw new ArgumentNullException(nameof(serviceSeller));
            ServiceBuyer = serviceBuyer ?? throw new ArgumentNullException(nameof(serviceBuyer));
            Shipper = shipper ?? throw new ArgumentNullException(nameof(shipper));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }
    }
}
