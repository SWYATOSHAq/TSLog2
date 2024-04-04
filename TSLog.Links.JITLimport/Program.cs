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

            try
            {
                Log.Information("Starting web application");
                
                using IHost host = CreateHostBuilder(args).Build();
                using var serviceScope = host.Services.CreateScope();
                var services = serviceScope.ServiceProvider;
                YardUtils.SetServiceProvider(services);
                await host.RunAsync();
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

                    services.AddTransient<IFtpMonitorService, FtpMonitorService>(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<FtpMonitorService>>();
                        return new FtpMonitorService(
                            ftpSettings["FtpServer"],
                            ftpSettings["FtpUsername"],
                            ftpSettings["FtpPassword"],
                            ftpSettings["RemotePath"],
                            logger);
                    });

                    services.AddTransient<IYardDbConnectService>(provider =>
                    {
                        var yardStorage = provider.GetRequiredService<IYardStorage>();
                        return new YardDbConnectService(yardStorage);
                    });
                    services.AddTransient<IExcelParsingService, ExcelParsingService>(provider =>
                    {
                        var yardDbConnectService = provider.GetRequiredService<IYardDbConnectService>();
                        var logger = provider.GetRequiredService<ILogger<ExcelParsingService>>();
                        return new ExcelParsingService(yardDbConnectService, logger);
                    });
                    services.AddHostedService<FtpMonitorBackgroundService>(provider =>
                    {
                        var ftpService = provider.GetRequiredService<IFtpMonitorService>();
                        var excelService = provider.GetRequiredService<IExcelParsingService>();
                        var logger = provider.GetRequiredService<ILogger<FtpMonitorBackgroundService>>();
                        return new FtpMonitorBackgroundService(ftpService, excelService, logger);
                    });
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