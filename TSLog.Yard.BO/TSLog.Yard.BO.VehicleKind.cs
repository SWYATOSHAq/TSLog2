using TSLog.Common.BO;

namespace TSLog.Yard.BO;

/// <summary>
/// Вид автомашины.
/// </summary>
public class VehicleKind : Entity
{
    private const double PalletLength = 1.2;
    private const double PalletWidth = 0.8;
    private const double PalletHeight = 1.7;

    private const int DefaultCapacity = 20000;
    private const short DefaultPallectsCount = 33;
    private const double DefaultBodyLength = 13.60;
    private const double DefaultBodyWidth = 2.45;
    /// <summary>
    /// Вместимость груза в кг.
    /// </summary>
    public int Capacity { get; private set; }
    /// <summary>
    /// Количество паллет (паллетовместимость).
    /// </summary>
    public short PalletsCount { get; private set; }
    /// <summary>
    /// Длина кузова в метрах.
    /// </summary>
    public double BodyLength { get; private set; }
    /// <summary>
    /// Ширина кузова в метрах.
    /// </summary>
    public double BodyWidth { get; private set; }
    /// <summary>
    /// Объём груза при максимальной загрузке.
    /// </summary>
    public double CargoVolume
    {
        get
        {
            return PalletLength * PalletWidth * PalletHeight * PalletsCount;
        }            
    }                
    public VehicleKind(short id, string code, string name, string shortName, string altName, string displayName,
        int capacity, short palletsCount, double bodyLength, double bodyWidth, 
        bool isActive = true, short displayOrder = -1,
        string note = "")
        : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note) 
    {
        Capacity = capacity > 0 ? capacity : DefaultCapacity;
        PalletsCount = palletsCount > 0 ? palletsCount : DefaultPallectsCount;
        BodyLength = bodyLength > 0 ? bodyLength : DefaultBodyLength ;
        BodyWidth = bodyWidth > 0 ? bodyWidth : DefaultBodyWidth;
    }
    private VehicleKind() : base()
    {
        Capacity = DefaultCapacity;
        PalletsCount = DefaultPallectsCount;
        BodyLength = DefaultBodyLength;
        BodyWidth = DefaultBodyWidth;
    }

    private static VehicleKind? _null = null;
    /// <summary>
    /// Null object.
    /// </summary>
    public static new VehicleKind Null
    {
        get
        {
            _null ??= new VehicleKind(-1, "NULL", "NULL", "NULL", "NULL", "NULL", 
                    DefaultCapacity, DefaultPallectsCount, DefaultBodyLength, DefaultBodyWidth, true, -1, "NULL");
            return _null;
        }
    }
    private static VehicleKind? _empty = null;
    /// <summary>
    /// Empty object.
    /// </summary>
    public static new VehicleKind Empty
    {
        get
        {
            _empty ??= new VehicleKind();
            return _empty;
        }
    }
}
