using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.MobileAppForBoxScan.Models;

public record DeleteBoxSSCCRequest
{
    public string teNum { get; init; }
    public string OrderId { get; init; }
}
