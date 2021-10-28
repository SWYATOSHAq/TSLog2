using System;
using System.Collections.Generic;
using System.Linq;

namespace TSLog.BO
{
    /// <summary>
    /// Приход товара на склад (заголовок)
    /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={1497144A-EAAA-437E-84A0-D1AF487D853E}
    /// </summary>
    public class Inbound : Entity
    {
        /// <summary>
        /// Товарная позиция прихода
        /// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7221368A-BD9A-4759-89BF-304C7B61931C}
        /// </summary>
        public class Line: LineBase
        {
            /// <summary>
            /// Цена за единицу измерения
            /// </summary>
            public double Price { get; private set; }
            /// <summary>
            /// Валюта (для цены)
            /// </summary>
            public Entity Currency { get; private set; }
            /// <summary>
            /// Сумма
            /// </summary>
            public double Sum => Price * Quantity;
            /// <summary>
            /// Планируемое количество
            /// </summary>
            public double PlanQuantity { get; private set; }
            /// <summary>
            /// Фактическое количество
            /// </summary>
            public double FactQuantity
            {
                get
                {
                    return Quantity;
                }
                private set
                {
                    Quantity = value;
                }
            }
            /// <summary>
            /// (OKAI) Код ОКЕИ
            /// </summary>
            public string OKEI { get; set; }
            /// <summary>
            /// (Info) Дополнительная информация  Возможно, по согласованию, 
            /// включать информацию в виде {BOX=002564}{TYPE=RZN}{COMMENT=комментарий}
            /// </summary>
            public string Info { get; private set; }
            /// <summary>
            /// Номер партии 
            /// </summary>
            public string BatchNumber { get; set; }
            /// <summary>
            /// Альтернативный номер партии
            /// </summary>
            public string BatchNumber2 { get; set; }
            private readonly IDictionary<LogisticUnit, double> _logisticUnits = null;
            public Line(int number, Product product, double price, Entity currency,
                double planQuantity, double factQuantity, MeasureUnit measureUnit, string note = "")
                : base(number, product, factQuantity, measureUnit)
            {                
                Price = price > 0.0 ? price : 0.0;
                if (currency == null && price > 0.0)
                    throw new ArgumentNullException(nameof(currency));
                Currency = price > 0.0 ? currency : Null;
                PlanQuantity = planQuantity;
                _logisticUnits = new Dictionary<LogisticUnit, double>();
                Info = !string.IsNullOrWhiteSpace(note) ? note.Trim() : string.Empty;
            }
            public void AddLogisticUnit(LogisticUnit logisticUnit, double quantity)
            {
                if (quantity <= 0.0)
                    return;
                if (logisticUnit == null)
                    throw new ArgumentNullException(nameof(logisticUnit));
                if (!_logisticUnits.ContainsKey(logisticUnit))
                    _logisticUnits.Add(logisticUnit, quantity);
                else
                    _logisticUnits[logisticUnit] += quantity;
            }
        }
        /// <summary>
        /// Идентификатор
        /// </summary>       
        new public string Id
        {
            get
            {
                return Code;
            }
        }
        /// <summary>
        /// (CustDocGUID) Код GUID номера документа
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// (WeaKlaNam) Класс поступающего заказа на товар
        /// </summary>
        public string Kind { get; set; }
        /// <summary>
        ///  Номер
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Номер прихода в системе партнёра
        /// </summary>
        public string PartnerNumber { get; set; }
        /// <summary>
        /// Статус (состояние)
        /// </summary>
        public Entity State { get; private set; }
        /// <summary>
        /// (EcOperation) Тип документа
        /// </summary>
        public OperationType Operation { get; set; }
        /// <summary>
        /// Партнёр (покупатель наших складских и логистических услуг)
        /// </summary>
        public LegalEntity Partner { get; set; }
        /// <summary>
        /// (CustVendId) Код контрагента (поставщика/грузополучателя)
        /// </summary>
        public LegalEntity Vendor { get; set; }
        /// <summary>
        /// Отметка о дате и времени (дата формирования документа)
        /// </summary>
        public DateTime Date { get; private set; }
        /// <summary>
        /// Плановая дата и время начала приемки
        /// </summary>
        public DateTime PlanAcceptanceBegin { get; set; }
        /// <summary>
        /// Плановая дата и время окончания приемки
        /// </summary>
        public DateTime PlanAcceptanceEnd { get; set; }
        /// <summary>
        /// Фактическая дата и время начала приемки
        /// </summary>
        public DateTime FactAcceptanceBegin { get; set; }
        /// <summary>
        /// Фактическая дата и время окончания приемки
        /// </summary>
        public DateTime FactAcceptanceEnd { get; set; }
        private readonly IDictionary<Product, Line> _lines = null;
        private readonly IDictionary<string, LogisticUnit> _logisticUnits = null;
        public Inbound(string id, string number, DateTime date, LegalEntity partner, string partnerNumber, Entity state, string note = "")
            : base(-1, id, string.Empty, string.Empty, string.Empty, string.Empty, note)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("number is empty");
            Number = number.Trim();
            Date = date;
            Partner = partner ?? throw new ArgumentNullException(nameof(partner));
            if (string.IsNullOrWhiteSpace(partnerNumber))
                throw new ArgumentException("partnerNumber is empty");
            PartnerNumber = partnerNumber.Trim();
            State = state ?? throw new ArgumentNullException(nameof(state));
            _lines = new Dictionary<Product, Line>();
            _logisticUnits = new Dictionary<string, LogisticUnit>();
        }
        /// <summary>
        /// Добавляет товарную позицию
        /// </summary>
        /// <param name="line">Товарная позиция</param>
        public void AddLine(Line line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
                _lines[line.Product].Quantity += line.Quantity;
        }
        /// <summary>
        /// Добавляет несколько товарных позиций
        /// </summary>
        /// <param name="lines">Товарные позиции</param>
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
        public Line[] Lines => _lines.Values.ToArray();
        /// <summary>
        /// Добавить место
        /// </summary>
        /// <param name="logisticUnit">Место</param>
        public void AddLogisticUnit(LogisticUnit logisticUnit)
        {
            if (logisticUnit == null)
                throw new ArgumentNullException(nameof(logisticUnit));
            if (!_logisticUnits.ContainsKey(logisticUnit.Sscc))
                _logisticUnits.Add(logisticUnit.Sscc, logisticUnit);
            else
                throw new ArgumentException($"logistic unit {logisticUnit.Sscc} already exists");
        }
        /// <summary>
        /// Добавить места
        /// </summary>
        /// <param name="logisticUnits">Места</param>
        public void AddLogisticUnits(LogisticUnit[] logisticUnits)
        {
            if (logisticUnits == null)
                throw new ArgumentNullException(nameof(logisticUnits));
            foreach (var logisticUnit in logisticUnits)
                AddLogisticUnit(logisticUnit);
        }
        /// <summary>
        /// Места прихода
        /// </summary>
        public LogisticUnit[] LogisticUnits => _logisticUnits.Values.ToArray();
    }
}
