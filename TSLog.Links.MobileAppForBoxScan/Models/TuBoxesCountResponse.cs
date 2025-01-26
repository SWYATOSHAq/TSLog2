namespace TSLog.Links.MobileAppForBoxScan.Models;

public class TuBoxesCountResponse
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
    /// Количество коробов на ТЕ (собранных, НЕ ОТСКАНИРОВАННЫХ!).
    /// </summary>
    public required int BoxesCount { get; init; }
}
