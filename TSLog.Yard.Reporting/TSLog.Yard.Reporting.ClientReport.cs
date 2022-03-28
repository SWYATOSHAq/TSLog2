using TSLog.Common.BO;
using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Reporting
{
    /// <summary>
    ///  Отчёт по партнёру за период
    /// </summary>
    public class ClientReport
    {
        /// <summary>
        /// Заголовок отчёта
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Партнёр
        /// </summary>
        public Partner Partner { get; private set; }
        /// <summary>
        /// Дата (и время) начала периода
        /// </summary>
        public DateTime FromDate { get; private set; }
        /// <summary>
        /// Дата (и вермя) окончания периода
        /// </summary>
        public DateTime ToDate { get; private set; }
        public ClientReport(IYardStorage storage, Partner partner, DateTime fromDate, DateTime toDate)
        {
            Partner = partner != null && partner != Partner.Empty ? partner : Partner.Null;
            FromDate = fromDate;
            ToDate = toDate;

            Create(storage);
        }
        /// <summary>
        /// Элемент отчёта
        /// </summary>
        public class Element
        {
            public Position Position { get; private set; }
            public int LaborCost { get; set; } = 0;
            public double Cost { get; set; } = 0.0;
            public MeasureUnit CostCurrency { get; set; }
            public Element(Position position, int laborCost, double cost, MeasureUnit costCurrency)
            {
                Position = position;
                LaborCost = laborCost;
                Cost = cost;
                CostCurrency = costCurrency;
            }
        }
        /// <summary>
        /// Список элементов отчёта
        /// </summary>
        public IList<Element> Elements { get; private set; } = new List<Element>();
        /// <summary>
        /// Создание отчёта
        /// </summary>
        /// <param name="storage">"Хранилище" данных</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Create(IYardStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            Elements.Clear();

            var rur = storage.GetMeasureUnit("RUR");

            var interval = new Interval(FromDate, ToDate);
            List<Partner> partners = new List<Partner>();
            if (Partner != null && Partner != Partner.Null && Partner != Partner.Empty)
                partners.Add(Partner);
            var operations = storage.GetOperations(FromDate, ToDate, partners.ToArray());
            var elements = new Dictionary<Position, Element>();
            foreach (var operation in operations)
            {
                var share = interval.OperationShare(operation);
                var positions = operation.Positions;
                var laborCosts = operation.PositionsLaborCosts();
                var costs = operation.PositionsCosts();
                foreach (var position in positions)
                {
                    if (!elements.ContainsKey(position))
                        elements.Add(position, new Element(position, 0, 0.0, rur));
                    var element = elements[position];
                    element.LaborCost += (int)Math.Round(laborCosts[position] * share);
                    element.Cost += costs[position] * share;
                    element.CostCurrency = rur;
                }
            }
            foreach (var element in elements.Values)
                Elements.Add(element);
        }
    }
}