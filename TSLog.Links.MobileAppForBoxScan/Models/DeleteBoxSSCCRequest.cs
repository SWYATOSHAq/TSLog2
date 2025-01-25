namespace TSLog.Links.MobileAppForBoxScan.Models;

public record DeleteBoxSSCCRequest
{
    public required string TeNum { get; init; }
    public required string OrderId { get; init; }
}
