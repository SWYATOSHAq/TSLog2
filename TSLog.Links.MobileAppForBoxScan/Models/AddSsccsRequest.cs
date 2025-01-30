namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP запроса на добавление НАБОРА SSCC.
/// </summary>
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
    /// Набор SSCC коробов.
    /// </summary>
    public required string[] Ssccs { get; init; }
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string UserName { get; init; }
}
