namespace TSLog.Links.MobileAppForBoxScan.Models;

public class TuBoxesCountResponse
{
    public required string Message { get; init; }
    public required string TuId { get; init; }
    public required int BoxesCount { get; init; }
}
