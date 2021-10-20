using System;
using System.Collections.Generic;

namespace TSLog.Links
{
    public class OrderNotofocation : Message
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
        public DateTime OrderDate { get; set; }
        public DateTime OrderPalnDeliveryDate { get; set; }

        public OrderNotofocation(long id, char kind, string type, byte version, string body = null)
            : base(id, kind, type, version, body)
        {
            _lines = new Dictionary<string, Line>();
        }

        private IDictionary<string, Line> _lines = null;
        public void AddLine(Line line) 
        {
            if (!_lines.ContainsKey(line.ProductId))
                _lines.Add(line.ProductId, line);
            else
                _lines[line.ProductId].Quantity += line.Quantity;
        }
    }
}
