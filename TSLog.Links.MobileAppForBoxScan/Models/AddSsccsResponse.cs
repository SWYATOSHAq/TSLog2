namespace TSLog.Links.MobileAppForBoxScan.Models;
public class AddSsccsResponse
{
    public required string Message { get; init; }
    public required string OrderId { get; init; }
    public required string TuIdId { get; init; }
    public int SsccsCount { get; init; }
}
