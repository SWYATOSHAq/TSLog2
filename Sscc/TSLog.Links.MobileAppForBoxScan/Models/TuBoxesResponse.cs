namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа для получения коробов на ТЕ в расходе (заказе).
/// </summary>
public class TuBoxesResponse
{
    /// <summary>
    /// Сообщение о результате.
    /// </summary>
    public required string Message { get; init; }
    /// <summary>
    /// Идентификатор ТЕ.
    /// </summary>
    public required string TuId { get; init; }
    /// <summary>
    /// Короба на ТЕ в расходе (заказе).
    /// </summary>
    public required string[] Boxes { get; init; }
}
