using TSLog.Common.BO;
using TSLog.LegalEntities.BO;
using TSLog.Partners.BO;
using TSLog.Products.BO;
using TSLog.WarehouseActions.BO;

namespace TSLog.Outgoes.BO;
/// <summary>
/// Расход.
/// Шапка.
/// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={7896B268-8251-47A6-95CC-6603BDD743D4}
/// Позиция.
/// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={2874B98C-FCA4-4398-964A-E8ACB90EA6FA}
/// </summary>
public class Outgo : WarehouseAction
{
    /// <summary>
    /// Товарная позиция расхода
    /// </summary>
    public class Line: LineBase
    {
        /// <summary>
        /// Цена (за единицу товара)
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Валюта.
        /// </summary>
        public Entity Currency { get; set; }

        /// <summary>
        /// Сумма.
        /// </summary>
        public double Sum => Price * base.FactQuantity;

        /// <summary>
        /// Код ОКЕИ.
        /// </summary>
        public string OKEI { get; set; } = string.Empty;

        /// <summary>
        /// Плановое количество.
        /// </summary>
        public new double PlanQuantity
        {
            get
            {
                return base.PlanQuantity;
            }
            set
            {
                base.PlanQuantity = value > 0.0 ? value : 0.0;
            }
        }

        /// <summary>
        /// Фактическое количество
        /// </summary>
        public new double FactQuantity
        {
            get
            {
                return base.FactQuantity;
            }
            set
            {
                base.FactQuantity = value > 0.0 ? value : 0.0;
            }
        }

        /// <summary>
        /// Дополнительная информация (структурированная или нет).
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// Номер партии.
        /// </summary>
        public string BatchNumber { get; set; } = string.Empty;

        /// <summary>
        /// Альтернативный номер партии.
        /// </summary>
        public string BatchNumber2 { get; set; } = string.Empty;

        private readonly Dictionary<LogisticUnit, double> _logisticUnits = [];

        public Line(int number, Product product, double price, Entity? currency,
            double planQuantity, double factQuantity, MeasureUnit measureUnit)
            : base(number, product, planQuantity, factQuantity, measureUnit)
        {
            Price = price > 0.0 ? price : 0.0;
            if (price > 0.0 && currency == null)
                throw new ArgumentNullException(nameof(currency));
            Currency = price > 0.0 && currency != null ? currency : Null;
            PlanQuantity = planQuantity > 0.0 ? planQuantity : 0.0;
        }

        /// <summary>
        /// Добавляет логистическую единицу.
        /// </summary>
        /// <param name="logisticUnit">Логистическая единица.</param>
        /// <param name="quantity">Количество товара на добавляемой логистической единице.</param>
        public void AddLogisticUnit(LogisticUnit logisticUnit, double quantity)
        {
            if (quantity <= 0.0)
                return;
            ArgumentNullException.ThrowIfNull(logisticUnit);
            if (!_logisticUnits.ContainsKey(logisticUnit))
                _logisticUnits.Add(logisticUnit, quantity);
            else
                _logisticUnits[logisticUnit] += quantity;
        }
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public new string Id
    {
        get
        {
            return Code;
        }
        set
        {
            Code = !string.IsNullOrWhiteSpace(value) ? value.Trim() : string.Empty;
        }
    }

    public string GUID { get; set; } = string.Empty;

    /// <summary>
    /// Номер в системе партнёра (клиента).
    /// </summary>
    public string PartnerNumer { get; private set; }

    /// <summary>
    /// Статус (состояние).
    /// </summary>
    public Entity State { get; set; }

    /// <summary>
    /// Тип.
    /// </summary>
    public Entity Type { get; set; } = Null;

    /// <summary>
    /// Конечный получатель заказа.
    /// </summary>
    public LegalEntity Vendor { get; set; } = LegalEntity.Null;

    /// <summary>
    /// Тип документа.
    /// </summary>
    public OperationType Operation { get; set; }

    /// <summary>
    /// Дата (формирования).
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Плановая дата и время начала отгрузки.
    /// </summary>
    public DateTime PlanShippingBegin { get; set; }

    /// <summary>
    /// Плановая дата и время окончания отгрузки.
    /// </summary>
    public DateTime PlanShippingEnd { get; set; }

    /// <summary>
    /// Фактическая дата и время начала отгрузки.
    /// </summary>
    public DateTime FactShippingBegin { get; set; }

    /// <summary>
    /// Фактическая дата и время окончания отгрузки.
    /// </summary>
    public DateTime FactShippingEnd { get; set; }

    /// <summary>
    /// Наименование точки доставки.
    /// </summary>
    public string DeliveryName { get; set; } = string.Empty;

    /// <summary>
    /// Индекс адреса доставки.
    /// </summary>
    public string DeliveryPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Город доставки.
    /// </summary>
    public string DeliveryCity { get; set; } = string.Empty;

    /// <summary>
    /// Улица и дом доставки (строка 1).
    /// </summary>
    public string DeliveryAddress1 { get; set; } = string.Empty;

    /// <summary>
    /// Улица и дом доставки (строка 2).
    /// </summary>
    public string DeliveryAddress2 { get; set; } = string.Empty;

    /// <summary>
    /// Комментарий к доставке. Возможно, по согласованию, включать текст в виде {BOX=002564}{TYPE=RZN}{COMMENT = комментарий}.
    /// </summary>
    public string DeliveryComment { get; set; } = string.Empty;

    private readonly IDictionary<string, LogisticUnit> _logisticUnits = new Dictionary<string, LogisticUnit>();

    public Outgo(string id, string number, DateTime date, Partner partner, string partnerNumber, Entity state, string note = "")
        : base(ActionKind.Outgo, id, number, partner, partnerNumber, DateTime.MinValue, DateTime.MinValue, true, note)
    {
        Date = date;
        Partner = partner ?? throw new ArgumentNullException(nameof(partner));
        if (string.IsNullOrWhiteSpace(partnerNumber))
            throw new ArgumentException("partnerNumber is empty");
        PartnerNumer = partnerNumber;
        State = state ?? throw new ArgumentNullException(nameof(state));
        Note = note;
    }

    /// <summary>
    /// Добавляет товарную позицию.
    /// </summary>
    /// <param name="line">Товарная позиция</param>
    public void AddLine(Line line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (!_lines.TryGetValue(line.Product, out LineBase? value))
            _lines.Add(line.Product, line);
        else
            value.FactQuantity += line.FactQuantity;
    }

    /// <summary>
    /// Добавляет товарные позиции.
    /// </summary>
    /// <param name="lines">Товарные позиции</param>
    public void AddLines(Line[] lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        foreach (var line in lines)
            AddLine(line);
    }

    /// <summary>
    /// Товарные позиции расхода.
    /// </summary>
    public new Line[] Lines
    {
        get
        {
            var lines = new List<Line>();
            foreach (var line in _lines.Values)
                lines.Add((Line)line);
            return lines.ToArray();
        }
    }

    /// <summary>
    /// Добавялет место.
    /// </summary>
    /// <param name="logisticUnit">Место</param>
    public void AddLogisticUnit(LogisticUnit logisticUnit)
    {
        ArgumentNullException.ThrowIfNull(logisticUnit);
        if (!_logisticUnits.ContainsKey(logisticUnit.Sscc))
            _logisticUnits.Add(logisticUnit.Sscc, logisticUnit);
        else
            throw new ArgumentException($"logistic unit {logisticUnit.Sscc} already exists");
    }

    /// <summary>
    /// Добавляет места.
    /// </summary>
    /// <param name="logisticUnits">Места</param>
    public void AddLogisticUnits(LogisticUnit[] logisticUnits)
    {
        ArgumentNullException.ThrowIfNull(logisticUnits);
        foreach (var logisticUnit in logisticUnits)
            AddLogisticUnit(logisticUnit);
    }

    /// <summary>
    /// Места.
    /// </summary>
    public LogisticUnit[] LogisticUnits => [.. _logisticUnits.Values];
}
