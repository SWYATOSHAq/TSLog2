namespace TSLog.Common.BO;
/// <summary>
/// Складская услуга.
/// </summary>
public class Service : Entity
{
    public string KindCode { get; private set; }
    /// <summary>
    /// Единица измерения.
    /// </summary>
    public MeasureUnit DefaultMeasure { get; private set; } 
    public string GroupName { get; private set; }
    public Service(int id, string kindCode, string code, string name, MeasureUnit measure, string groupName)
        : base(id, code, name, name, name, name, true, -1, name)
    {
        KindCode = !string.IsNullOrWhiteSpace(kindCode) ? kindCode.Trim().ToUpper() : string.Empty;
        DefaultMeasure = measure;
        GroupName = !string.IsNullOrWhiteSpace(groupName) ? groupName.Trim() : string.Empty;
    }
}
