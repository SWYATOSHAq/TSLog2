namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа для получения количества ТЕ в расходе (заказе).
/// </summary>
public class OutgoTusResponse
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
    /// ТЕ в заказе (с не нулевым количеством коробов).
    /// </summary>
    public required string[] Tus { get; init; }
}
