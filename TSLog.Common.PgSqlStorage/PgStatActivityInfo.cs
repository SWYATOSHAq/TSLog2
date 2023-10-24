using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Common.PgSqlStorage;

internal class PgStatActivityInfo
{
    public long DatId { get; set; }
    public string DatName { get; set; }
    public int Pid { get; set; }
    public long UseSysId { get; set; }
    public string UseName { get; set; }
    public string? ClientIPAdress { get; set; }
    public int ClientPort { get; set; }
    public string State { get; set; }
    public string Query { get; set; }
}
