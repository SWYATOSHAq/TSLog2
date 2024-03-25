using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Serilog;
using TSLog.Links.JITLimport.Services;
using TSLog.Links.Storage;
using System;
using TSLog.Yard;

namespace TSLog.Links.JITLimport
{
    internal class Program
    {
        static IConfiguration? Configuration { get; set; }
        static async Task Main(string[] args)
        {
            const string DefaultCommonSchema = "common";
            const string DefaultStaffSchema = "staff";
            const string DefaultLegalEntitiesSchema = "legal_entities";
            const string DefaultPartnersSchema = "partners";
            const string DefaultYardSchema = "links";

            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(); // Add Serilog as a logging provider
            });
            try
            {
                Log.Information("Starting web application");
                var isDebug = Configuration.GetValue<bool>("IsDebug");

                var oracleConnectionSettings = isDebug
                    ? Configuration.GetSection("OracleConnectionDebugSettings")
                    : Configuration.GetSection("OracleConnectionSettings");
                var oracleTns = oracleConnectionSettings.GetValue<string>("DataSource") ?? string.Empty;
                var oracleUserName = oracleConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
                var oraclePassword = oracleConnectionSettings.GetValue<string>("Password") ?? string.Empty;
                var oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema") ?? DefaultCommonSchema;
                var oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema") ?? DefaultStaffSchema;
                var oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema")?? DefaultLegalEntitiesSchema;
                var oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema") ?? DefaultPartnersSchema;

                var pgsqlConnectionSettings = isDebug
                    ? Configuration.GetSection("PostgreSqlConnectionDebugSettings")
                    : Configuration.GetSection("PostgreSqlConnectionSettings");
                var pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host") ?? string.Empty;
                var pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
                var pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password") ?? string.Empty;
                var pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database") ?? string.Empty;
                var pgsqlPooling = pgsqlConnectionSettings.GetValue<bool>("Pooling");
                var pgsqlMaxPoolSize = pgsqlConnectionSettings.GetValue<int>("MaxPoolSize");
                var pgsqlConnectionIdleLifetime = pgsqlConnectionSettings.GetValue<int>("ConnectionIdleLifetime");
                var pgsqlConnectionPruningInterval = pgsqlConnectionSettings.GetValue<int>("ConnectionPruningInterval");
                var pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema") ?? DefaultYardSchema;

                //var storage = new LinksStorage(oracleTns, pgsqlHost, pgsqlDatabase, loggerFactory);
                var storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase, loggerFactory);
                storage.MaxRowsCount = Configuration.GetValue<int>("MaxRowsCount");

                storage.ConnectOracle(oracleUserName, oraclePassword);
                storage.SetOracleCommonSchema(oracleCommonSchema);
                storage.SetOracleStaffSchema(oracleStaffSchema);
                storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
                storage.SetOraclePartnersSchema(oraclePartnersSchema);
                storage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
                storage.SetPgsqlYardSchema(pgsqlYardSchema);

                var temp = storage.GetOperation(10042780);
                //builder.Services.AddSingleton<ILinksStorage>(storage);



                var ftpServer = Configuration["FtpSettings:ftpServer"];
                var ftpUsername = Configuration["FtpSettings:ftpUsername"];
                var ftpPassword = Configuration["FtpSettings:ftpPassword"];
                var remotePath = Configuration["FtpSettings:remotePath"];

                var ftpService = new FtpMonitorService(ftpServer, ftpUsername, ftpPassword, remotePath);
                var excelService = new ExcelParsingService();

                var files = ftpService.CheckForXlsFiles();
                await ftpService.DownloadFilesAsync(files);
                ftpService.MoveRemoteFilesToArchive(FtpMonitorService.ARCHIEVE_PATH);
                string downloadedFilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedXlsFiles");
                excelService.ParseExcelFiles(downloadedFilesDir);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
        }
    }
}