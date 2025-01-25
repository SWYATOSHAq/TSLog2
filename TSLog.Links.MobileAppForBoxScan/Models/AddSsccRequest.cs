namespace TSLog.Links.MobileAppForBoxScan.Models;

public record AddSsccRequest
{
    public required string OutgoId { get; init; }
    public required string TuId { get; init; }
    public required string Sscc { get; init; }
    public required string UserName { get; init; }
}


