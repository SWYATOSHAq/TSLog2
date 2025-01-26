namespace TSLog.Links.MobileAppForBoxScan.Models;
public class OutgoScannedTusCountResponse
{
    public required string Message { get; init; }
    public required string OutgoId { get; init; }
    public required int TusCount { get; init; }
}
