using System;
using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    public class OutboundNotification : Message
    {
        public class Line
        {
            public int Number { get; set; }
            public Product Product { get; set; }
            public double Quantity { get; set; }
            public string measureUnitCode { get; set; }
        }

        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string PartnerOrderNumber { get; set; }

        private IDictionary<Product, Line> _lines = null;
        public OutboundNotification(long id, char kind, string type, byte version, string body = null)
            : base(id, kind, type, version, body)
        {
            _lines = new Dictionary<Product, Line>();
        }
        public void AddLine(Line line) 
        {
            if (line == null)
                throw new ArgumentNullException("line");
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
                _lines[line.Product].Quantity += line.Quantity;
        }
    }
}
