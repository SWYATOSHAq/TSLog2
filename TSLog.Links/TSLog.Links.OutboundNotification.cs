using System;
using System.Collections.Generic;
using System.Linq;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Заказ на сборку (и доставку) товара
    /// </summary>
    public class OutboundNotification : Message
    {
        /// <summary>
        /// Товарная позиция ответа на заказ, уведомления о старте сборки, уведомления об окончании сборки, уведомления об отгрузке
        /// </summary>
        public class Line: LineBase
        {
            public double PlanQuantity
            {
                get { return Quantity; }
                set { Quantity = value; } 
            }
            public double FactQuantity { get; set; }
            public Line(int number, Product product, double quantity)
                : base(number, product, quantity)
            { }
        }

        public string OutboundId { get; set; }
        public string OutboundNumber { get; set; }
        public string PartnerOutboundNumber { get; set; }
        /// <summary>
        /// Статус (состояние)
        /// </summary>
        public string State { get; set; }
        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }
        private IDictionary<Product, Line> _lines = null;

        public OutboundNotification(long id, byte version, string body = null)
            : base(id, 'O', "OUTBOUND_NOTIFICATION", version, body)
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
        public void AddLines(Line[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException("lines");
            foreach (var line in lines)
                AddLine(line);
        }
        public Line[] Lines
        {
            get
            {
                return _lines.Values.ToArray();
            }
        }
    }
}
