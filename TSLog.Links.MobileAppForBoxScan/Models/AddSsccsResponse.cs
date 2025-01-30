namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа на добавление НАБОРА SSCC. 
/// </summary>
public class AddSsccsResponse
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
    /// Идентификатор транспортной единицы (ТЕ).
    /// </summary>
    public required string TuId { get; init; }
    /// <summary>
    /// Количество сохранённых SSCC коробов.
    /// </summary>
    public int SsccsCount { get; init; }
}
