namespace TSLog.Links.MobileAppForBoxScan.Models;

public class ErrorResponse
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required int Status { get; init; }
    public required string Detail { get; init; }
}
