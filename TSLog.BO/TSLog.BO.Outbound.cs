using System;
using System.Collections.Generic;
using System.Linq;

namespace TSLog.BO
{
    /// <summary>
    /// Расход
    /// Шапка
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7896B268-8251-47A6-95CC-6603BDD743D4}
    /// Позиция
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={2874B98C-FCA4-4398-964A-E8ACB90EA6FA}
    /// </summary>
    public class Outbound : Entity
    {
        /// <summary>
        /// Товарная позиция расхода
        /// </summary>
        public class Line: LineBase
        {
            public double PlanQuantity { get; set; }
            public double FactQuantity { get; set; }
            public Line(int number, Product product, double quantity, MeasureUnit measureUnit)
                : base(number, product, quantity, measureUnit)
            { }
        }
        public string GUID { get; set; }
        public string Number { get; set; }
        public string PartnerNumer { get; set; }
        public LegalEntity Partner { get; set; }
        public LegalEntity Vendor { get; set; }
        /// <summary>
        /// Тип документа 
        /// </summary>        
        public OperationType Operation { get; set; }
        /// <summary>
        /// Плановая дата отгрузки в формате dd.mm.yyyy hh24:mi:ss 
        /// </summary>
        public DateTime ShipDate { get; set; }
        /// <summary>
        /// Наименование точки доставки 
        /// </summary>
        public string DeliveryName { get; set; }
        /// <summary>
        /// Индекс адреса доставки 
        /// </summary>
        public string DeliveryZip { get; set; }
        /// <summary>
        /// Город доставки 
        /// </summary>
        public string DeliveryCity { get; set; }
        /// <summary>
        /// Улица и дом доставки (строка 2)
        /// </summary>
        public string DeliveryAddress1 { get; set; }
        /// <summary>
        /// Улица и дом доставки (строка 1) 
        /// </summary>
        public string DeliveryAddress2 { get; set; }
        /// <summary>
        /// Комментарий к доставке. Возможно, по согласованию, включать текст в виде {BOX=002564}{TYPE=RZN}{COMMENT = комментарий}
        /// </summary>
        public string DeliveryComment { get; set; }
        public DateTime Date { get; set; }
        private readonly IDictionary<Product, Line> _lines = null;
        private readonly IDictionary<string, LogisticUnit> _logisticUnits = null;
        public Outbound(string id) : base(-1, id)
        {
            _lines = new Dictionary<Product, Line>();
            _logisticUnits = new Dictionary<string, LogisticUnit>();
        }
        public void AddLine(Line line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
                _lines[line.Product].Quantity += line.Quantity;
        }
        public void AddLines(Line[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));
            foreach (var line in lines)
                AddLine(line);
        }
        /// <summary>
        /// Товарные позиции расхода
        /// </summary>
        public Line[] Lines => _lines.Values.ToArray();
    }
}
