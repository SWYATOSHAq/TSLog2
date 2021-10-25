using System;
using System.Collections.Generic;
using System.Linq;
using TSLog.BO;

namespace TSLog.Links
{
    public class OutboundNotification : Message
    {
        /// <summary>
        /// Товарная позиция ответа на заказ, уведомления о старте сборки, уведомления об окончании сборки, уведомления об отгрузке
        /// </summary>
        public class Line : LineBase
        {
            public Line(int number, Product product, double quantity) 
                : base(number, product, quantity)
            { }
        }

        public string OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string PartnerOrderNumber { get; set; }

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
