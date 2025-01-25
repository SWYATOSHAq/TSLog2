namespace TSLog.Common.PgSqlStorage;

internal class PgStatActivityInfo
{
    public long DatId { get; set; }
    public required string DatName { get; set; }
    public int Pid { get; set; }
    public long UseSysId { get; set; }
    public required string UseName { get; set; }
    public string? ClientIPAdress { get; set; }
    public int ClientPort { get; set; }
    public required string State { get; set; }
    public required string Query { get; set; }
}
