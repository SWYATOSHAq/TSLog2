namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа на добавление SSCC. 
/// </summary>
public class AddSsccResponse
{
    /// <summary>
    /// Сообщение о результате.
    /// </summary>
    public required string Message { get; init; }
    /// <summary>
    /// Идентификатор расхода (заказа).
    /// </summary>
    public required string OrderId { get; init; }
    /// <summary>
    /// Идентификатор ТЕ.
    /// </summary>
    public required string TuIdId { get; init; }
}
