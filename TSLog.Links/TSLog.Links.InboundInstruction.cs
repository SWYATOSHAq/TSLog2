using System;
using System.Collections.Generic;
using System.Linq;
using TSLog.BO;

namespace TSLog.Links
{
    /// <summary>
    /// Сообщение запрос на приход товара на склад
    /// </summary>
    public class InboundInstruction : Message
    {
        /// <summary>
        /// Товарная позиция запроса на приход товара на склад
        /// </summary>
        public class Line: LineBase
        {
            public LogisticUnit[] LogisticUnits { get; private set; }
            public Line(int number, Product product, double quantity, LogisticUnit[] logisticUnits)
                : base(number, product, quantity)
            {
                LogisticUnits = logisticUnits;
            }
        }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }        
        public string PartnerInboundNumber { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime PlanDeliveryBegin { get; set; }
        public DateTime PlanDeliveryEnd { get; set; }

        public LegalEntity ServiceSeller { get; set; }
        public LegalEntity ServiceBuyer { get; set; }

        public LegalEntity Shipper { get; set; }
        public LegalEntity Receiver { get; set; }

        private IDictionary<string, LogisticUnit> _logisticUnits = null;
        private IDictionary<Product, Line> _lines = null;
        public void AddLogisticUnit(LogisticUnit logisticUnit)
        {
            if (logisticUnit == null)
                throw new ArgumentNullException("logisticUnit");
            _logisticUnits.Add(logisticUnit.Sscc, logisticUnit);
        }
        public void AddLogisticUnits(LogisticUnit[] logisticUnits)
        {
            if (logisticUnits == null)
                throw new ArgumentNullException("logisticUnits");
            foreach (var logisticUnit in logisticUnits)
                AddLogisticUnit(logisticUnit);
        }
        public LogisticUnit[] LogisticUnits
        {
            get
            {
                return _logisticUnits.Values.ToArray();
            }
        }
        public void AddLine(Line line)
        {
            if (line == null)
                throw new ArgumentNullException("line");
            // TODO: check logistic unit
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
                // TODO: recalculate to other measure unit
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
        public InboundInstruction(byte version, string body = null) 
            : base(-1, 'I', "INBOUND_INSTRUCTION", version, body)
        {
            _logisticUnits = new Dictionary<string, LogisticUnit>();
            _lines = new Dictionary<Product, Line>();
        }
    }
}
