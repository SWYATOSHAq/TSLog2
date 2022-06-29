using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Products.BO;

namespace TSLog.WarehouseActions.BO
{
    /// <summary>
    /// Складское действо (приход, расход, ...)
    /// </summary>
    public class WarehouseAction : Entity
    {
        /// <summary>
        /// Идентификаитор
        /// </summary>
        public new string Id
        {
            get
            {
                return Code;
            }
            protected set
            {
                Code = value;
            }
        }
        /// <summary>
        /// Номер
        /// </summary>
        public string Number { get; private set; }
        /// <summary>
        /// Вид (приход, расход, ...)
        /// </summary>
        public ActionKind Kind { get; private set; }
        /// <summary>
        /// Ключ - вид + идентификатор
        /// </summary>
        public string Key
        {
            get
            {
                var kind = Kind == ActionKind.Outgo ? "O" : "I";
                return $"{kind}{Id}";
            }
        }
        /// <summary>
        /// Партнёр - покупатель наших складских и логистических услуг
        /// </summary>
        public Partner Partner { get; set; }
        /// <summary>
        /// Идентификатор складского действа в системе партнёра
        /// </summary>
        public string PartnerActionId { get; set; }
        /// <summary>
        /// Плановая дата (и время) складского действа
        /// </summary>
        public DateTime PlanActionDate { get; set; }
        protected readonly IDictionary<Product, LineBase> _lines = new Dictionary<Product, LineBase>();
        public WarehouseAction(ActionKind kind, string id, string number, Partner partner, string partnerActionId, DateTime planActionDate,
            bool isActive, string note)
            : base(-1, id, string.Empty, string.Empty, string.Empty, string.Empty, isActive, -1, note)
        {
            //if (string.IsNullOrWhiteSpace(number))
            //    throw new ArgumentException("number is empty");
            Number = number.Trim();
            Kind = kind;
            Partner = partner;
            PartnerActionId = partnerActionId;
            PlanActionDate = planActionDate;
        }
        private WarehouseAction() : base()
        {
            Number = string.Empty;
            Partner = Partner.Empty;
            PartnerActionId = string.Empty;
        }

        /// <summary>
        /// Добавляет товарную позицию
        /// </summary>
        /// <param name="line">Товарная позиция</param>
        public void AddLine(LineBase line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
                _lines[line.Product].FactQuantity += line.FactQuantity;
        }
        /// <summary>
        /// Добавляет товарные позиции
        /// </summary>
        /// <param name="lines">Товарные позиции</param>
        public void AddLines(LineBase[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));
            foreach (var line in lines)
                AddLine(line);
        }
        /// <summary>
        /// Товарные позиции расхода
        /// </summary>
        public LineBase[] Lines => _lines.Values.ToArray();
        /// <summary>
        /// Суммарный объём
        /// </summary>
        public double Volume
        {
            get
            {
                var volume = 0.0;
                foreach (var line in _lines.Values)
                    volume += line.PlanVolume;
                return volume;
            }
        }
        public MeasureUnit VolumeMeasureunit
        {
            get
            {
                return _lines.Count > 0 ? _lines.Values.ToArray()[0].VolumeMeasureUnit : MeasureUnit.Null;
            }
        }
        /// <summary>
        /// Суммарный вес нетто
        /// </summary>
        public double NetWeight
        {
            get
            {
                var netWeight = 0.0;
                foreach (var line in _lines.Values)
                    netWeight += line.FactNetWeight;
                return netWeight;
            }
        }
        /// <summary>
        /// Суммарный вес брутто
        /// </summary>
        public double GrossWeight
        {
            get
            {
                var grossWeight = 0.0;
                foreach (var line in _lines.Values)
                    grossWeight += line.FactGrossWeight;
                return grossWeight;
            }
        }
        public MeasureUnit WeightMeasureUnit
        {
            get
            {
                return _lines.Count > 0 ? _lines.Values.ToArray()[0].WeightMeasureUnit : MeasureUnit.Null;
            }
        }
        /// <summary>
        /// Плановое количество паллет
        /// </summary>
        public double PlanPalletsCount
        {
            get
            {
                var count = 0.0;
                foreach (var line in _lines.Values)
                    count += line.PlanQuantity / line.PalletStandard;
                return count;
            }
        }
        /// <summary>
        /// "Фактическое" колличество паллет (рассчёт по фактичесому количеству)
        /// </summary>
        public double FactPalletsCount
        {
            get
            {
                var count = 0.0;
                foreach (var line in _lines.Values)
                    count += line.FactQuantity / line.PalletStandard;
                return count;
            }
        }

        private static WarehouseAction? _null = null;
        /// <summary>
        /// Null object
        /// </summary>
        public static new WarehouseAction Null
        {
            get
            {
                if (_null == null)
                    _null = new WarehouseAction(ActionKind.Income, "NULL", "NULL", Partner.Null, "NULL", DateTime.MinValue, true, "NULL");
                return _null;
            }
        }
        private static WarehouseAction? _empty = null;
        /// <summary>
        /// Empty object
        /// </summary>
        public static new WarehouseAction Empty
        {
            get
            {
                if (_empty == null)
                    _empty = new WarehouseAction();
                return _empty;
            }
        }
    }
}