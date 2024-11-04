namespace TSLog.Links.MobileAppForBoxScan.Models;

public record DeleteBoxSSCCRequest
{
    public string teNum { get; init; }
    public string OrderId { get; init; }
}
