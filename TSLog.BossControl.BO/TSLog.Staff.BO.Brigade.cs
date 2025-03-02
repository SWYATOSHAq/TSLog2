using TSLog.Common.BO;

namespace TSLog.Staff.BO;

/// <summary>
/// Бригада.
/// </summary>
public class Brigade: Entity
{
    /// <summary>
    /// Вид.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    public Brigade(int id, string kind, string name, string note)
        : base(id, string.Empty, name, name, name, name, true, -1, note)
    {
        Kind = !string.IsNullOrWhiteSpace(kind) ? kind.Trim() : string.Empty;
    }
}
