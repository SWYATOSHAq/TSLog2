namespace TSLog.Links.MobileAppForBoxScan.Models;
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
    /// SSCC короба.
    /// </summary>
    public required string TuIdId { get; init; }
    /// <summary>
    /// Количество сохранённых SSCC коробов.
    /// </summary>
    public int SsccsCount { get; init; }
}
