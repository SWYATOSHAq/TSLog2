using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Заказ на сборку (и доставку) товара
    /// </summary>
    public class OutboundNotification : Message
    {
        /// <summary>
        /// Отгрузка товара со склада
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

        public OutboundNotification(long id, string number, byte version, DateTime date, Outbound outbound,
            LegalEntity sender, LegalEntity recipient,
            LegalEntity serviceSeller, LegalEntity serviceBuyer, LegalEntity shipper, LegalEntity receiver,
            string note = "")
            : base(id, number,  'O', "OUTBOUND_NOTIFICATION", version, date, sender, recipient, note)
        {
            Outbound = outbound ?? throw new ArgumentNullException(nameof(outbound));
            ServiceSeller = serviceSeller ?? throw new ArgumentNullException(nameof(serviceSeller));
            ServiceBuyer = serviceBuyer ?? throw new ArgumentNullException(nameof(serviceBuyer));
            Shipper = shipper ?? throw new ArgumentNullException(nameof(shipper));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }
    }
}
