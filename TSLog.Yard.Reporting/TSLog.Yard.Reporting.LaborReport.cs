using TSLog.Partners.BO;
using TSLog.Staff.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Reporting
{
    /// <summary>
    /// Отчёт по трудозатратам
    /// </summary>
    public class LaborReport
    {
        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public LaborReport(DateTime fromDate, DateTime toDate)
        {
            FromDate = fromDate;
            ToDate = toDate;
            _elements = new Dictionary<string, Element>();
        }
        public class Element : IComparable<Element>
        {
            public Partner Partner { get; private set; }
            public Position Position { get; private set; }
            #region Boss control
            public int BcWorkersCount { get; set; } = 0;
            public double BcLaborCost { get; set; } = 0;
            public double BcNormalizedLaborCost
            {
                get
                {
                    return BcWorkersCount > 0 ? BcLaborCost / BcWorkersCount : BcWorkersCount;
                }
            }
            #endregion

            #region Yard by standard
            public double YSPlanLaborCost { get; set; } = 0;
            public double YSFactLaborCost { get; set; } = 0;
            #endregion

            #region Yard by allocated workers
            public int YAWorkersCount { get; set; } = 0;
            public double YAPlanLaborCost { get; set; } = 0;
            public double YAFactLaborCost { get; set; } = 0;
            public double YANormalizedPlanLaborCost
            {
                get
                {
                    return BcWorkersCount > 0 ? YAPlanLaborCost / BcWorkersCount : 0;
                }
            }
            public double YANormalizedfactLaborCost
            {
                get
                {
                    return BcWorkersCount > 0 ? YAFactLaborCost / BcWorkersCount : 0;
                }
            }
            #endregion
            public Element(Partner partner, Position postion)
            {
                Partner = partner;
                Position = postion;
            }
            public int CompareTo(Element? other)
            {
                if (other == null)
                    return 1;
                if (Partner.Id == other.Partner.Id)
                    return Position.Id.CompareTo(other.Position.Id);
                else
                    return Partner.Id.CompareTo(other.Partner.Id);
            }
        }
        private readonly IDictionary<string, Element> _elements;
        public Element[] Elements
        {
            get 
            {
                var elements = _elements.Values.ToArray();
                Array.Sort(elements);
                return elements;
            }
        }
        public void Create(IYardStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            _elements.Clear();

            var lines = storage.GetBcLabocCosts(FromDate, ToDate);
            foreach (var line in lines)
            {
                var partner = storage.GetPartner(line.PartnerId);
                var position = line.PositionId > 0 ? storage.GetPosition(line.PositionId) : Position.Null;
                var element = new Element(partner, position);
                element.BcLaborCost = line.LaborCost;
                element.BcWorkersCount = line.WorkersCount;

                var key = $"{partner.Id}_{position.Id.ToString()}";
                _elements.Add(key, element);
            }

            lines = storage.GetYardStandardLabocCosts(FromDate, ToDate);
            foreach (var line in lines)
            {
                var partner = storage.GetPartner(line.PartnerId);
                var position = line.PositionId > 0 ? storage.GetPosition(line.PositionId) : Position.Null;
                var key = $"{partner.Id}_{position.Id.ToString()}";
                if (_elements.ContainsKey(key))
                {
                    var element = _elements[key];
                    element.YSPlanLaborCost = line.PlanLaborCost;
                    element.YSFactLaborCost = line.FactLaborCost;
                }
                else
                {
                    var element = new Element(partner, position);
                    element.YSPlanLaborCost = line.PlanLaborCost;
                    element.YSFactLaborCost = line.FactLaborCost;
                    _elements.Add(key, element);
                }
            }

            lines = storage.GetYardAllocatedLabocCosts(FromDate, ToDate);
            foreach (var line in lines)
            {
                var partner = storage.GetPartner(line.PartnerId);
                var position = line.PositionId > 0 ? storage.GetPosition(line.PositionId) : Position.Null;
                var key = $"{partner.Id}_{position.Id.ToString()}";
                if (_elements.ContainsKey(key))
                {
                    var element = _elements[key];
                    element.YAWorkersCount = line.WorkersCount;
                    element.YAPlanLaborCost = line.PlanLaborCost;
                    element.YAFactLaborCost = line.FactLaborCost;
                }
                else
                {
                    var element = new Element(partner, position);
                    element.YAWorkersCount = line.WorkersCount;
                    element.YAPlanLaborCost = line.PlanLaborCost;
                    element.YAFactLaborCost = line.FactLaborCost;
                    _elements.Add(key, element);
                }
            }
        }
    }
}