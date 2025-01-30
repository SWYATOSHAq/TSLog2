namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа для получения количества ТЕ с отсканированными SSCC для расхода (заказа).
/// </summary>
public class OutgoScannedTusCountResponse
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
    /// Количество ОТСКАНИРОВАНЫХ ТЕ в заказе (ТЕ считается отсканированой если на ней отсканированы все короба!).
    /// </summary>
    public required int TusCount { get; init; }
}
