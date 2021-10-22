using System;
using System.Collections.Generic;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Уведомление о приёмке прихода
    /// </summary>
    public class InboundNotification : Message
    {
        public class Line: LineBase
        {
            Line(int id, Product product, double quantity)
                    : base(id, product, quantity)
            { }
        }

        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string InboundNumber { get; set; }
        public string PartnerInboundNumber { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime InboundPlanDeliveryDate { get; set; }
        public DateTime FactInboundDeliveryDate { get; set; }

        private IDictionary<string, LogisticUnit> _logisticUnits = null;
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
        public InboundNotification(long id, byte version, string body = null) 
            : base(id, 'O', "INBOUND_NOTIFICATION", version, body)
        {
            _logisticUnits = new Dictionary<string, LogisticUnit>();
            _lines = new Dictionary<Product, Line>();
        }
    }
}
