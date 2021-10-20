using System.Collections.Generic;

namespace TSLog.Links
{
    public class OrderResponse : Message
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

        private IDictionary<string, Line> _lines = null;
        public OrderResponse(long id, char kind, string type, byte version, string body = null)
            : base(id, kind, type, version, body)
        {
            //
        }
        public void AddLine(Line line) 
        {
            if (!_lines.ContainsKey(line.ProductId))
                _lines.Add(line.ProductId, line);
            else
                _lines[line.ProductId].Quantity += line.Quantity;
        }
    }
}
