using System;
using System.Collections.Generic;
using System.Linq;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    ///  Заказ на сборку и доставку
    /// </summary>
    public class OutboundInstruction : Message
    {
        public Outbound Outbound { get; set; }
        public DateTime PlanDeliveryBegin { get; set; }
        public DateTime PlanDeliveryEnd { get; set; }

        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }
        public OutboundInstruction(long id, byte version, string body = null)
            : base(id, 'I', "OUTBOUND_INSTRUCTION", version, body)
        { }
    }
}
