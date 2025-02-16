using TSLog.Common.BO;

namespace TSLog.Yard.BO;

/// <summary>
/// Логист.
/// </summary>
public class Logist : Entity
{
    public Logist(long id, string code, string name = "", string shortName = "", string altName = "", string displayName = "",
        bool isActive = true, short displayOrder = -1,
        string note = "")
        : base(id, code, name, shortName, altName, displayName, isActive, displayOrder, note)
    { }
    private Logist() : base()
    { }
    private static Logist? _null = null;

    /// <summary>
    /// Null object.
    /// </summary>
    public static new Logist Null
    {
        get
        {
            _null ??= new Logist(-1, "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, "NULL");
            return _null;
        }
    }
    private static Logist? _empty = null;

    /// <summary>
    /// Empty object.
    /// </summary>
    public static new Logist Empty
    {
        get
        {
            _empty ??= new Logist();
            return _empty;
        }
    }
}
