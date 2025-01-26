namespace TSLog.Links.MobileAppForBoxScan.Models;
internal class AddSsccsRequest
{
    public required string OutgoId { get; init; }
    public required string TuId { get; init; }
    public required string[] Ssccs { get; init; }
    public required string UserName { get; init; }
}
