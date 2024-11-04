namespace TSLog.Links.MobileAppForBoxScan.Models;

public record BoxSSCCRequest
{
    public string OrderId { get; init; }
    public string BoxSSCCCode { get; init; }
    public string teNum { get; init; }
    public string UserName { get; init; }
}


