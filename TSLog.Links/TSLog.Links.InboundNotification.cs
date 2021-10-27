using System;
using System.Collections.Generic;
using System.Linq;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Уведомление о приёмке прихода
    /// </summary>
    public class InboundNotification : Message
    {
        public Inbound Inbound { get; set; }
        public string InboundInstructionNumber { get; set; }
        public string PartnerInboundNumber { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime InboundPlanDeliveryDate { get; set; }
        public DateTime FactInboundDeliveryDate { get; set; }
        /// <summary>
        /// Статус (состояние)
        /// </summary>
        public string State { get; set; }
        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }
        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public string Note { get; set; }

        public InboundNotification(long id, byte version, string body = null)
            : base(id, 'O', "INBOUND_NOTIFICATION", version, body)
        { }
    }
}
