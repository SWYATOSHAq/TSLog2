namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа на проверку расхода (заказа).
/// </summary>
public class CheckOutgoResponse
{
    /// <summary>
    /// Сообщение о результате.
    /// </summary>
    public required string Message { get; set; }
    /// <summary>
    /// Идентификатор расхода (заказа).
    /// </summary>
    public required string OutgoId { get; set; }
}
