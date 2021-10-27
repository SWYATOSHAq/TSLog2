using System;
using System.Collections.Generic;
using System.Linq;

namespace TSLog.BO
{
    /// <summary>
    /// Приход товара на склад
    /// </summary>
    public class Inbound : Entity
    {
        /// <summary>
        /// Товарная позиция прихода
        /// Шапка
        /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={1497144A-EAAA-437E-84A0-D1AF487D853E}
        /// Позиция
        /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7221368A-BD9A-4759-89BF-304C7B61931C}
        /// </summary>
        public class Line: LineBase
        {
            public double PlanQuantity { get; set; }
            public double FactQuantity { get; set; }
            /// <summary>
            /// Цена за единицу измерения
            /// </summary>
            public double Price { get; set; }
            /// <summary>
            /// Валюта (для цены)
            /// </summary>
            public Entity Currency { get; set; }
            /// <summary>
            /// Сумма
            /// </summary>
            public double Sum => Price * Quantity;
            /// <summary>
            /// (OKAI) Код ОКЕИ
            /// </summary>
            public string OKEI { get; set; }
            /// <summary>
            /// (Info) Дополнительная информация  Возможно, по согласованию, 
            /// включать информацию в виде {BOX=002564}{TYPE=RZN}{COMMENT=комментарий}
            /// </summary>
            public string Info { get; set; }
            /// <summary>
            /// Партия 
            /// </summary>
            public string BatchNumber { get; set; }
            /// <summary>
            /// Партия2 (?)
            /// </summary>
            public string BatchNumber2 { get; set; }
            public Line(int number, Product product, double price, Entity currency, 
                double quantity, MeasureUnit measureUnit) 
                : base(number, product, quantity, measureUnit)
            {
                if (currency == null)
                    throw new ArgumentNullException(nameof(currency));
                Price = price;
                Currency = currency;
            }
        }
        /// <summary>
        /// (CustDocGUID) Код GUID номера документа
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// (WeaKlaNam) Класс поступающего заказа на товар
        /// </summary>
        public string InboundKind { get; set; }
        /// <summary>
        ///  Номер
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Номер прихода в системе партнёра
        /// </summary>
        public string PartnerNumber { get; set; }
        /// <summary>
        /// (EcOperation) Тип документа
        /// </summary>
        public OperationType Operation { get; set; }
        /// <summary>
        /// (CustDocId) Номер документа клиента
        /// </summary>
        public string ClientDocId { get; set; }
        /// <summary>
        /// (MndNr) Цифровой идентификатор клиента
        /// </summary>
        public LegalEntity Partner { get; set; }
        /// <summary>
        /// (CustVendId) Код контрагента (поставщика/грузополучателя)
        /// </summary>
        public LegalEntity Vendor { get; set; }
        /// <summary>
        /// отметка о дате и времени (дата формирования документа)
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// (ArrivalPlanDate) Плановая дата приемки в формате dd.mm.yyyy hh24:mi:ss 
        /// </summary>
        public DateTime ArrivalPlanDate { get; set; }
        /// <summary>
        /// отметка о дате и времени (дата потенциального прихода)
        /// </summary>
        public DateTime PotentialInboundDate { get; set; }        
        private readonly IDictionary<Product, Line> _lines = null;
        private readonly IDictionary<string, LogisticUnit> _logisticUnits = null;
        public Inbound(string id, string number, string note = "", bool isActive = true)
            : base(-1, id, string.Empty, string.Empty, string.Empty, string.Empty, note, isActive)
        {
            Number = number;
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
        /// Товарные позиции прихода
        /// </summary>
        public Line[] Lines
        {
            get
            {
                return _lines.Values.ToArray();
            }
        }
    }
}
