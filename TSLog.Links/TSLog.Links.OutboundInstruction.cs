using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    ///  Заказ на сборку и доставку
    /// </summary>
    public class OutboundInstruction : Message
    {
        /// <summary>
        /// Отгрузка со склада
        /// </summary>
        public Outbound Outbound { get; private set; }

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

        public OutboundInstruction(long id, string number, byte version, DateTime date, Outbound outbound,
            LegalEntity sender, LegalEntity recipient,
            LegalEntity serviceBuyer, LegalEntity serviceSeller, LegalEntity shipper, LegalEntity receiver,
            string note = "")
            : base(id, number, 'I', "OUTBOUND_INSTRUCTION", version, date, sender, recipient)
        {
            Outbound = outbound ?? throw new ArgumentNullException(nameof(outbound));
            ServiceSeller = serviceSeller ?? throw new ArgumentNullException(nameof(serviceSeller));
            ServiceBuyer = serviceBuyer ?? throw new ArgumentNullException(nameof(serviceBuyer));
            Shipper = shipper ?? throw new ArgumentNullException(nameof(shipper));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }
    }
}