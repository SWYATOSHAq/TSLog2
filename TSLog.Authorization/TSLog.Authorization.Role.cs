namespace TSLog.Authorization;

/// <summary>
/// Роль (совокупность прав и привилегий).
/// </summary>
public class Role
{
    /// <summary>
    /// Код.
    /// </summary>
    public string Code { get; private set; }
    /// <summary>
    /// Название.
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Название группы.
    /// </summary>
    public string GroupName { get; private set; }
    /// <summary>
    /// Примечания и комментарии на естественном языке.
    /// </summary>
    public string Note { get; private set; }
    public Role(string code, string name, string groupName, string note = "")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is empty");
        Code = code.Trim().ToUpper();
        Name = name.Trim();
        GroupName = groupName;
        Note = note;
    }
    /// <summary>
    /// Набор прав (кодов прав) данной роли.
    /// </summary>
    public HashSet<string> Rights { get; private set; } = [];
}
