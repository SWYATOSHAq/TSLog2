namespace TSLog.Links
{
    public class ShippingNotification : Message
    {
        public class Line
        {
            public int Number { get; set; }
            public string ProductId { get; set; }
            public double Quantity { get; set; }
            public string measureUnitCode { get; set; }
        }

        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string PartnerOrderNumber { get; set; }
        public ShippingNotification(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            //
        }
    }
}
