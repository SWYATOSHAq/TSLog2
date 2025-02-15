namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа для получения количества ТЕ в расходе (заказе).
/// </summary>
public class OutgoTusCountResponse
{
    /// <summary>
    /// Сообщение о результате.
    /// </summary>
    public required string Message { get; init; }
    /// <summary>
    /// Идентификатор расхода (заказа).
    /// </summary>
    public required string OutgoId { get; init; }
    /// <summary>
    /// Количество ТЕ в заказе (с не нулевым количеством коробов).
    /// </summary>
    public required int TusCount { get; init; }
}
