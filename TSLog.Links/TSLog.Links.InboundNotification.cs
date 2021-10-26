using System;
using System.Collections.Generic;
using System.Linq;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Уведомление о приёмке прихода
    /// </summary>
    public class InboundNotification : Message
    {
        /// <summary>
        /// Товарная позиция уведомления о приёмке
        /// </summary>
        public class Line: LineBase
        {
            /// <summary>
            /// Планируемое количество
            /// </summary>
            public double PlanQuantity
            {
                get { return Quantity; }
                set { Quantity = value; }
            }
            /// <summary>
            /// Фактическое количество
            /// </summary>
            public double FactQuantity { get; set; }
            public Line(int id, Product product, double quantity)
                    : base(id, product, quantity)
            { }
        }

        public LegalEntity Partner { get; set; }
        public string InboundId { get; set; }
        public string InboundInstructionNumber { get; set; }
        public string PartnerInboundNumber { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime InboundPlanDeliveryDate { get; set; }
        public DateTime FactInboundDeliveryDate { get; set; }
        /// <summary>
        /// Статус (состояние)
        /// </summary>
        public string State { get; set; }
        public LegalEntity ServiceBuyer { get; set; }
        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }
        /// <summary>
        /// Примечания и комментарии на естественном языке
        /// </summary>
        public string Note { get; set; }

        private IDictionary<string, LogisticUnit> _logisticUnits = null;
        private IDictionary<Product, Line> _lines = null;        
        public InboundNotification(long id, byte version, string body = null) 
            : base(id, 'O', "INBOUND_NOTIFICATION", version, body)
        {
            _logisticUnits = new Dictionary<string, LogisticUnit>();
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
