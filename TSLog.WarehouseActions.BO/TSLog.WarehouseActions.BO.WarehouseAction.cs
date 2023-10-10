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
        public DateTime InsertDate { get; set; }
        protected readonly IDictionary<Product, LineBase> _lines = new Dictionary<Product, LineBase>();
        public WarehouseAction(ActionKind kind, string id, string number, Partner partner, string partnerActionId,
            DateTime planActionDate, DateTime insertDate, bool isActive, string note)
            : base(-1, id, string.Empty, string.Empty, string.Empty, string.Empty, isActive, -1, note)
        {
            Number = number.Trim();
            Kind = kind;
            Partner = partner;
            PartnerActionId = partnerActionId;
            PlanActionDate = planActionDate;
            InsertDate = insertDate;
        }
        private WarehouseAction() : base()
        {
            Number = string.Empty;
            Partner = Partner.Empty;
            PartnerActionId = string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is WarehouseAction))
                return false;

            var otherAction = (WarehouseAction)obj;
            return Key.Equals(otherAction.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        /// <summary>
        /// Добавляет товарную позицию.
        /// </summary>
        /// <param name="line">Товарная позиция</param>
        public void AddLine(LineBase line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            if (!_lines.ContainsKey(line.Product))
                _lines.Add(line.Product, line);
            else
            {
                var sameLine = _lines[line.Product];
                sameLine.PlanQuantity += line.PlanQuantity;
                sameLine.FactQuantity += line.FactQuantity;
                sameLine.PlanNetWeight += line.PlanNetWeight;
                sameLine.FactNetWeight += line.FactNetWeight;
                sameLine.PlanGrossWeight += line.PlanGrossWeight;
                sameLine.FactGrossWeight += line.FactGrossWeight;
                sameLine.PlanVolume += line.PlanVolume;
                sameLine.FactVolume += line.FactVolume;
                sameLine.PlanBoxesCount += line.PlanBoxesCount;
                sameLine.FactBoxesCount += line.FactBoxesCount;
                sameLine.PlanPalletsCount += line.PlanPalletsCount;
                sameLine.FactPalletsCount += line.FactPalletsCount;
            }
        }
        /// <summary>
        /// Добавляет товарные позиции.
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
        /// Суммарный плановый объём.
        /// </summary>
        public double PlanVolume
        {
            get
            {
                var volume = 0.0;
                foreach (var line in _lines.Values)
                    volume += line.PlanVolume;
                return volume;
            }
        }
        /// <summary>
        /// Суммарный фактический объём.
        /// </summary>
        public double FactVolume
        {
            get
            {
                var volume = 0.0;
                foreach (var line in _lines.Values)
                    volume += line.FactVolume;
                return volume;
            }
        }
        /// <summary>
        /// Единица измерения объёма.
        /// </summary>
        public MeasureUnit VolumeMeasureunit
        {
            get
            {
                return _lines.Count > 0 ? _lines.Values.ToArray()[0].VolumeMeasureUnit : MeasureUnit.Null;
            }
        }
        /// <summary>
        /// Суммарный плановый вес нетто.
        /// </summary>
        public double PlanNetWeight
        {
            get
            {
                var planNetWeight = 0.0;
                foreach (var line in _lines.Values)
                    planNetWeight += line.PlanNetWeight;
                return planNetWeight;
            }
        }
        /// <summary>
        /// Суммарный плановый вес брутто.
        /// </summary>
        public double PlanGrossWeight
        {
            get
            {
                var planGrossWeight = 0.0;
                foreach (var line in _lines.Values)
                    planGrossWeight += line.PlanGrossWeight;
                return planGrossWeight;
            }
        }
        /// <summary>
        /// Суммарный фактический вес нетто.
        /// </summary>
        public double FactNetWeight
        {
            get
            {
                var factNetWeight = 0.0;
                foreach (var line in _lines.Values)
                    factNetWeight += line.FactNetWeight;
                return factNetWeight;
            }
        }
        /// <summary>
        /// Суммарный фактический вес брутто.
        /// </summary>
        public double FactGrossWeight
        {
            get
            {
                var factGrossWeight = 0.0;
                foreach (var line in _lines.Values)
                    factGrossWeight += line.FactGrossWeight;
                return factGrossWeight;
            }
        }
        /// <summary>
        /// Единица измерения веса.
        /// </summary>
        public MeasureUnit WeightMeasureUnit
        {
            get
            {
                return _lines.Count > 0 ? _lines.Values.ToArray()[0].WeightMeasureUnit : MeasureUnit.Null;
            }
        }
        /// <summary>
        /// Плановое количество паллет.
        /// </summary>
        public double PlanPalletsCount
        {
            get
            {
                var count = 0.0;
                foreach (var line in _lines.Values)
                    count += line.PalletStandard > 0 ? line.PlanQuantity / line.PalletStandard : 0.0;
                return count;
            }
        }
        /// <summary>
        /// "Фактическое" колличество паллет (рассчёт по фактичесому количеству).
        /// </summary>
        public double FactPalletsCount
        {
            get
            {
                var count = 0.0;
                foreach (var line in _lines.Values)
                    count += line.PalletStandard > 0 ? line.FactQuantity / line.PalletStandard : 0.0;
                return count;
            }
        }
        /// <summary>
        /// Плановое количество коробок.
        /// </summary>
        public double PlanBoxeCount
        {
            get
            {
                var count = 0.0;
                foreach (var line in _lines.Values)
                    count += line.BoxStandard > 0 ? line.PlanQuantity / line.BoxStandard : 0.0;
                return count;
            }
        }
        /// <summary>
        /// Фактическое количество коробок.
        /// </summary>
        public double FactBoxeCount
        {
            get
            {
                var count = 0.0;
                foreach (var line in _lines.Values)
                    count += line.BoxStandard > 0 ? line.FactQuantity / line.BoxStandard : 0.0;
                return count;
            }
        }
        /// <summary>
        /// Количество ТЕ в заказе (приходе).
        /// </summary>
        public int TusCount { get; set; } = 0;
        private static WarehouseAction? _null = null;
        /// <summary>
        /// Null object.
        /// </summary>
        public static new WarehouseAction Null
        {
            get
            {
                if (_null == null)
                    _null = new WarehouseAction(ActionKind.Income, "NULL", "NULL", Partner.Null, "NULL", DateTime.MinValue, DateTime.MinValue, true, "NULL");
                return _null;
            }
        }
        private static WarehouseAction? _empty = null;
        /// <summary>
        /// Empty object.
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
