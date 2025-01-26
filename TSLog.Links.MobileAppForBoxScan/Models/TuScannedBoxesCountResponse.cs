namespace TSLog.Links.MobileAppForBoxScan.Models;
public class TuScannedBoxesCountResponse
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
    /// Количество отсканированных коробов на ТЕ.
    /// </summary>
    public required int BoxesCount { get; init; }
}
