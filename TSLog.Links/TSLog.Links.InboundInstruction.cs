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
        /// Товарная позиция запроса на приход товара на склад
        /// </summary>
        public Inbound Inbound { get; set; }
        public DateTime PlanDeliveryBegin { get; set; }
        public DateTime PlanDeliveryEnd { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }
        public InboundInstruction(byte version, string body = null)
            : base(-1, 'I', "INBOUND_INSTRUCTION", version, body)
        { }
    }
}
