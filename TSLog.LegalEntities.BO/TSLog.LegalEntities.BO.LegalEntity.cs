using TSLog.Common.BO;

namespace TSLog.LegalEntities.BO;

/// <summary>
/// Юридическое лицо (организация)
/// https://tehskladlogistic.sharepoint.com/sites/msteams_99bc8a/_layouts/15/Doc.aspx?OR=teams&action=edit&sourcedoc={DD97C915-2553-4B50-8753-EFDF79AEC6C0}
/// </summary>
public class LegalEntity : Entity
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public new string Id
    {
        get
        {
            return Code;
        }
    }

    /// <summary>
    /// Дополнительная информация (адрес)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительная информация
    /// </summary>
    public string Info1 { get; set; }

    /// <summary>
    /// Дополнительная информация
    /// </summary>
    public string Info2 { get; set; }

    public LegalEntity(string id, 
        string name = "", string shortName = "", string altName = "", string displayName = "", 
        string info1 = "", string info2 = "", bool isActive = true, long displayOrder = -1, 
        string note = "")
        : base(-1, id, name, shortName, altName, displayName, isActive, displayOrder, note)
    {
        Info1 = !string.IsNullOrWhiteSpace(info1) ? info1 : string.Empty;
        Info2 = !string.IsNullOrWhiteSpace(info2) ? info2 : string.Empty;
    }
    protected LegalEntity() : base()
    {
        Address = string.Empty;
        Info1 = string.Empty; 
        Info2 = string.Empty;
    }
    private static LegalEntity? _null = null;

    /// <summary>
    /// Null object
    /// </summary>
    public new static LegalEntity Null
    {
        get
        {
            if (_null == null)
                _null = new LegalEntity("NULL", "NULL", "NULL", "NULL", "NULL", "NULL", "NULL", true, -1, "NULL");
            return _null;
        }
    }
    private static LegalEntity? _empty = null;

    /// <summary>
    /// Empty object
    /// </summary>
    public static new LegalEntity Empty
    {
        get
        {
            if (_empty == null)
                _empty = new LegalEntity();
            return _empty;
        }
    }

}
