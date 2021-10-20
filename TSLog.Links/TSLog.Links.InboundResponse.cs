using System;

namespace TSLog.Links
{
    public class InboundResponse : Message
    {
        public class Line
        {
            public int Number { get; set; }
            public string ProductId { get; set; }
            public double Quantity { get; set; }
            public string measureUnitCode { get; set; }
        }

        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string InboundNumber { get; set; }
        public string PartnerInboundNumber { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime InboundPlanDeliveryDate { get; set; }
        public DateTime FactInboundDeliveryDate { get; set; }
        public InboundResponse(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            //
        }
    }
}
