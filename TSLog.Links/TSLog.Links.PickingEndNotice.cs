using System;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Заказ на сборку (и доставку) товара
    /// </summary>
    public class PickingEndNotice : Message
    {
        /// <summary>
        /// Отгрузка товара со склада
        /// </summary>
        public Outgo Outgo { get; private set; }

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

        public PickingEndNotice(long id, string number, byte version, DateTime date, Outgo outgo,
            LegalEntity sender, LegalEntity recipient,
            LegalEntity serviceSeller, LegalEntity serviceBuyer, LegalEntity shipper, LegalEntity receiver,
            string note = "")
            : base(id, number, 'O', "PICKING_END_NOTICE", version, date, sender, recipient, note)
        {
            Outgo = outgo ?? throw new ArgumentNullException(nameof(outgo));
            ServiceSeller = serviceSeller ?? throw new ArgumentNullException(nameof(serviceSeller));
            ServiceBuyer = serviceBuyer ?? throw new ArgumentNullException(nameof(serviceBuyer));
            Shipper = shipper ?? throw new ArgumentNullException(nameof(shipper));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }
    }
}