using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Заказ на сборку (и доставку) товара
    /// </summary>
    public class OutboundNotification : Message
    {
        public Outbound Outbound { get; set; }
        public string PartnerOutboundNumber { get; set; }
        /// <summary>
        /// Статус (состояние)
        /// </summary>
        public string State { get; set; }
        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }

        public OutboundNotification(long id, byte version, string body = null)
            : base(id, 'O', "OUTBOUND_NOTIFICATION", version, body)
        { }
    }
}
