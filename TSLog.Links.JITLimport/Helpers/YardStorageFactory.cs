using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Yard;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport.Helpers;

public static class YardStorageFactory
{
    public static IYardStorage CreateYardStorage(IConfiguration configuration, ILoggerFactory loggerFactory, bool isDebug)
    {
        const string DefaultCommonSchema = "common";
        const string DefaultStaffSchema = "staff";
        const string DefaultLegalEntitiesSchema = "legal_entities";
        const string DefaultPartnersSchema = "partners";
        const string DefaultYardSchema = "links";

        var oracleConnectionSettings = isDebug
            ? configuration.GetSection("OracleConnectionDebugSettings")
            : configuration.GetSection("OracleConnectionSettings");

        var oracleTns = oracleConnectionSettings.GetValue<string>("DataSource") ?? string.Empty;
        var oracleUserName = oracleConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
        var oraclePassword = oracleConnectionSettings.GetValue<string>("Password") ?? string.Empty;
        var oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema") ?? DefaultCommonSchema;
        var oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema") ?? DefaultStaffSchema;
        var oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema")?? DefaultLegalEntitiesSchema;
        var oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema") ?? DefaultPartnersSchema;

        var pgsqlConnectionSettings = isDebug
            ? configuration.GetSection("PostgreSqlConnectionDebugSettings")
            : configuration.GetSection("PostgreSqlConnectionSettings");


        var pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host") ?? string.Empty;
        var pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
        var pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password") ?? string.Empty;
        var pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database") ?? string.Empty;
        var pgsqlPooling = pgsqlConnectionSettings.GetValue<bool>("Pooling");
        var pgsqlMaxPoolSize = pgsqlConnectionSettings.GetValue<int>("MaxPoolSize");
        var pgsqlConnectionIdleLifetime = pgsqlConnectionSettings.GetValue<int>("ConnectionIdleLifetime");
        var pgsqlConnectionPruningInterval = pgsqlConnectionSettings.GetValue<int>("ConnectionPruningInterval");
        var pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema") ?? DefaultYardSchema;

        var yardStorage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase, loggerFactory);

        yardStorage.ConnectOracle(oracleUserName, oraclePassword);
        yardStorage.SetOracleCommonSchema(oracleCommonSchema);
        yardStorage.SetOracleStaffSchema(oracleStaffSchema);
        yardStorage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
        yardStorage.SetOraclePartnersSchema(oraclePartnersSchema);
        yardStorage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
        yardStorage.SetPgsqlYardSchema(pgsqlYardSchema);

        return yardStorage;
    }
}
