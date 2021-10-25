using System;
using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    ///  Заказ на сборку и доставку
    /// </summary>
    public class OutboundInstruction : Message
    {
        /// <summary>
        /// Товарная позиция заказа на сборку и доставку
        /// </summary>
        public class Line : LineBase
        {
            public Line(int number, Product product, double quantity = 0)
                : base(number, product, quantity)
            {
                //
            }
        }

        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string PartnerOrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PlanDeliveryBegin { get; set; }
        public DateTime PlanDeliveryEnd { get; set; }

        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }

        public OutboundInstruction(long id, byte version, string body = null)
            : base(id, 'I', "OUTBOUND_INSTRUCTION", version, body)
        {
            _lines = new Dictionary<Product, Line>();
        }

        private IDictionary<Product, Line> _lines = null;
        public void AddLine(Line line)
        {
            if (line == null)
                throw new ArgumentNullException("line");
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
                _lines[line.Product].Quantity += line.Quantity;
        }
        public void AddLines(Line[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException("lines");
            foreach (var line in lines)
                AddLine(line);
        }
    }
}
