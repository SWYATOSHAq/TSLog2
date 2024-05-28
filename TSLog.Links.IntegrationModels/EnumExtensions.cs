using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.IntegrationModels;

public static class EnumExtensions
{
    public static string ToStringValue(this AuaNameValues value)
    {
        return value switch
        {
            AuaNameValues.Retail => "RETAIL",
            AuaNameValues.Mono => "MONO",
            AuaNameValues.Mix => "MIX",
            AuaNameValues.Normal => "NORMAL",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}

