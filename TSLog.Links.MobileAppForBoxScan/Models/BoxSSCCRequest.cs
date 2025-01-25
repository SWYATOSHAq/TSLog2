namespace TSLog.Links.MobileAppForBoxScan.Models;

public record BoxSSCCRequest
{
    public required string OrderId { get; init; }
    public required string BoxSSCCCode { get; init; }
    public required string TeNum { get; init; }
    public required string UserName { get; init; }
}


