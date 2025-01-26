namespace TSLog.Links.MobileAppForBoxScan.Models;

public class ErrorResponse
{
    /// <summary>
    /// Тип ошибки.
    /// </summary>
    public required string Type { get; init; }
    /// <summary>
    /// Заголовок (краткое описание).
    /// </summary>
    public required string Title { get; init; }
    /// <summary>
    /// Код статуса HTTP.
    /// </summary>
    public required int Status { get; init; }
    /// <summary>
    /// Детальное описание ошибки.
    /// </summary>
    public required string Detail { get; init; }
}
