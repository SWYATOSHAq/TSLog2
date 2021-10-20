using System.Collections.Generic;

namespace TSLog.Links
{
    public class PickingStartNotification : Message
    {
        public class Line
        {
            public int Number { get; set; }
            public string ProductId { get; set; }
            public double Quantity { get; set; }
            public string measureUnitCode { get; set; }
        }

        public string PartnerId { get; set; }
        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string OrderPiskingStartDate { get; set; }
        public PickingStartNotification(long id, char kind, string type, byte version, string body = null) : base(id, kind, type, version, body)
        {
            //
        }

        private IDictionary<string, Line> _lines = null;

        public void AddLind(Line line) 
        {
            if (!_lines.ContainsKey(line.ProductId))
                _lines.Add(line.ProductId, line);
            else
                _lines[line.ProductId].Quantity += line.Quantity;
        }
    }
}
