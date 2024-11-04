namespace TSLog.Links.WebApiForBoxScan.Models;

public class OracleConnectionSettings
{
    public string? DataSource { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? CommonSchema { get; set; }
    public string? WmsSchema { get; set; }
    public string? SsccSchema { get; set; }
    public string? StaffSchema { get; set; }
    public string? LegalEntitiesSchema { get; set; }
    public string? PartnersSchema { get; set; }
}
