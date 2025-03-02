using TSLog.Staff.BO;
using TSLog.Yard.BO;

namespace TSLog.Yard.Reporting;

public class ShiftLaborReport
{
    public DateTime FromDate { get; private set; }
    public DateTime ToDate { get; private set; }
    public ShiftLaborReport(DateTime fromDate, DateTime toDate)
    {
        FromDate = fromDate;
        ToDate = toDate;
    }
    public class Element : IComparable<Element>
    {
        public Position Position { get; private set; }
        public double BcLaborCost { get; set; } = 0.0;
        public double BcStandardLaborCost { get; set; } = 0.0;
        public double BcEfficiency
        {
            get
            {
                return BcStandardLaborCost != 0 ? BcLaborCost / BcStandardLaborCost : 0.0;
            }
        }
        public double YLaborCost { get; set; } = 0.0;
        public double YG01LaborCost { get; set; } = 0.0;
        public double YG02LaborCost { get; set; } = 0.0;
        public double YG03LaborCost { get; set; } = 0.0;
        public double YG04LaborCost { get; set; } = 0.0;
        public double YG05LaborCost { get; set; } = 0.0;
        public double YG06LaborCost { get; set; } = 0.0;
        public double YG07LaborCost { get; set; } = 0.0;
        public double YG08LaborCost { get; set; } = 0.0;
        public double YG09LaborCost { get; set; } = 0.0;
        public double YG10LaborCost { get; set; } = 0.0;
        public double YG11LaborCost { get; set; } = 0.0;
        public double YG12LaborCost { get; set; } = 0.0;
        public double YG13LaborCost { get; set; } = 0.0;
        public double YG14LaborCost { get; set; } = 0.0;
        public double YG15LaborCost { get; set; } = 0.0;
        public double YG16LaborCost { get; set; } = 0.0;
        public Element(Position position)
        {
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }
        public int CompareTo(Element? other)
        {
            return other != null ? Position.Id.CompareTo(other.Position.Id) : 1;
        }
    }
    private readonly IDictionary<Position, Element> _elements = new Dictionary<Position, Element>();
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

        var lines = storage.GetBcShiftLabocCosts(FromDate);
        foreach (var line in lines)
        {
            var position = line.PositionId > 0 ? storage.GetPosition(line.PositionId) : Position.Null;
            var element = new Element(position);
            element.BcLaborCost = line.LaborCost;
            element.BcStandardLaborCost = line.StandardLaborCost;

            _elements.Add(position, element);
        }

        lines = storage.GetYardShiftLabocCosts(FromDate);
        foreach (var line in lines)
        {
            var position = line.PositionId > 0 ? storage.GetPosition(line.PositionId) : Position.Null;

            Element element;
            if (_elements.ContainsKey(position))
                element = _elements[position];
            else
            {
                element = new Element(position);
                _elements.Add(position, element);
            }
            element.YLaborCost = line.LaborCost;
            element.YG01LaborCost = line.G01LaborCost;
            element.YG02LaborCost = line.G02LaborCost;
            element.YG03LaborCost = line.G03LaborCost;
            element.YG04LaborCost = line.G04LaborCost;
            element.YG05LaborCost = line.G05LaborCost;
            element.YG06LaborCost = line.G06LaborCost;
            element.YG07LaborCost = line.G07LaborCost;
            element.YG08LaborCost = line.G08LaborCost;
            element.YG09LaborCost = line.G09LaborCost;
            element.YG10LaborCost = line.G10LaborCost;
            element.YG11LaborCost = line.G11LaborCost;
            element.YG12LaborCost = line.G12LaborCost;
            element.YG13LaborCost = line.G13LaborCost;
            element.YG14LaborCost = line.G14LaborCost;
            element.YG15LaborCost = line.G15LaborCost;
            element.YG16LaborCost = line.G16LaborCost;
        }
    }
}
