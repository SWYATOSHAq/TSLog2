using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Serilog;
using TSLog.Links.JITLimport.Services;
using TSLog.Links.Storage;
using System;
using TSLog.Yard;
using TSLog.Links.JITLimport.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.Yard.BO;

namespace TSLog.Links.JITLimport
{
    internal class Program
    {
        static IConfiguration? Configuration { get; set; }
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder.AddSerilog(); // Add Serilog as a logging provider
            //});
            try
            {
                Log.Information("Starting web application");
                //var host = Host.CreateDefaultBuilder()
                //    .ConfigureServices((context, services) =>
                //    {
                //        //services.AddTransient<>();
                //    })
                //    .Build();
                
                using IHost host = CreateHostBuilder(args).Build();
                using var serviceScope = host.Services.CreateScope();
                var services = serviceScope.ServiceProvider;

                //var isDebug = Configuration.GetValue<bool>("IsDebug");

                //var oracleConnectionSettings = isDebug
                //    ? Configuration.GetSection("OracleConnectionDebugSettings")
                //    : Configuration.GetSection("OracleConnectionSettings");
                //var oracleTns = oracleConnectionSettings.GetValue<string>("DataSource") ?? string.Empty;
                //var oracleUserName = oracleConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
                //var oraclePassword = oracleConnectionSettings.GetValue<string>("Password") ?? string.Empty;
                //var oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema") ?? DefaultCommonSchema;
                //var oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema") ?? DefaultStaffSchema;
                //var oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema")?? DefaultLegalEntitiesSchema;
                //var oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema") ?? DefaultPartnersSchema;

                //var pgsqlConnectionSettings = isDebug
                //    ? Configuration.GetSection("PostgreSqlConnectionDebugSettings")
                //    : Configuration.GetSection("PostgreSqlConnectionSettings");
                //var pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host") ?? string.Empty;
                //var pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
                //var pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password") ?? string.Empty;
                //var pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database") ?? string.Empty;
                //var pgsqlPooling = pgsqlConnectionSettings.GetValue<bool>("Pooling");
                //var pgsqlMaxPoolSize = pgsqlConnectionSettings.GetValue<int>("MaxPoolSize");
                //var pgsqlConnectionIdleLifetime = pgsqlConnectionSettings.GetValue<int>("ConnectionIdleLifetime");
                //var pgsqlConnectionPruningInterval = pgsqlConnectionSettings.GetValue<int>("ConnectionPruningInterval");
                //var pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema") ?? DefaultYardSchema;

                //var storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase, loggerFactory);
                //storage.MaxRowsCount = Configuration.GetValue<int>("MaxRowsCount");

                //storage.ConnectOracle(oracleUserName, oraclePassword);
                //storage.SetOracleCommonSchema(oracleCommonSchema);
                //storage.SetOracleStaffSchema(oracleStaffSchema);
                //storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
                //storage.SetOraclePartnersSchema(oraclePartnersSchema);
                //storage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
                //storage.SetPgsqlYardSchema(pgsqlYardSchema);

                //var yardDbConnectService = new YardDbConnectService(storage);
                //var temp = storage.GetOperation(10042780);
                //builder.Services.AddSingleton<ILinksStorage>(storage);



                //var ftpServer = Configuration["FtpSettings:ftpServer"];
                //var ftpUsername = Configuration["FtpSettings:ftpUsername"];
                //var ftpPassword = Configuration["FtpSettings:ftpPassword"];
                //var remotePath = Configuration["FtpSettings:remotePath"];

                var ftpService = services.GetRequiredService<IFtpMonitorService>();
                var excelService = services.GetRequiredService<IExcelParsingService>();
                var yardDbConnectService = services.GetRequiredService<IYardDbConnectService>();

                var temp = yardDbConnectService.GetYardOperation();

                //var ftpService = new FtpMonitorService(ftpServer, ftpUsername, ftpPassword, remotePath);
                //var excelService = new ExcelParsingService(yardDbConnectService);

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

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    var isDebug = hostContext.Configuration.GetValue<bool>("IsDebug");

                    services.AddSingleton<IYardStorage>(provider =>
                    {
                        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                        return YardStorageFactory.CreateYardStorage(configuration, loggerFactory, isDebug);
                    });

                    var ftpSettings = hostContext.Configuration.GetSection("FtpSettings");
                    services.AddTransient<IFtpMonitorService>(provider =>
                        new FtpMonitorService(
                            ftpSettings["FtpServer"],
                            ftpSettings["FtpUsername"],
                            ftpSettings["FtpPassword"],
                            ftpSettings["RemotePath"]));
                    
                    services.AddTransient<IYardDbConnectService>(provider =>
                    {
                        var yardStorage = provider.GetRequiredService<IYardStorage>();
                        return new YardDbConnectService(yardStorage);
                    });
                    services.AddTransient<IExcelParsingService, ExcelParsingService>();                    
                })
                .UseSerilog();

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
        }
    }
}