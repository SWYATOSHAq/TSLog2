namespace TSLog.Links.MobileAppForBoxScan.Models;
public class AddSsccsRequest
{
    /// <summary>
    /// Идентификатор расхода (заказа).
    /// </summary>
    public required string OutgoId { get; init; }
    /// <summary>
    /// Идентификатор ТЕ.
    /// </summary>
    public required string TuId { get; init; }
    /// <summary>
    /// SSCC короба.
    /// </summary>
    public required string[] Ssccs { get; init; }
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string UserName { get; init; }
}
