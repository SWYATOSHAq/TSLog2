using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TSLog.Links.BLCtobacco.Interfaces;
using TSLog.Links.BLCtobacco.Models;
using TSLog.Links.BLCtobacco.Services;

namespace TSLog.Links.BLCtobacco;

public class Program
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
            var isDebug = hostContext.Configuration.GetValue<bool>("IsDebug");
            services.Configure<TimeHostConfig>(hostContext.Configuration.GetSection("TimeHostConfig"));
            services.AddHttpClient();
            services.AddSingleton<IHttpClientService, HttpClientService>();            
            services.AddHostedService<TimedHostedService>();
            services.AddTransient<IDataProcessor, DataProcessor>();
        })
        .UseSerilog();

    static void BuildConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", 
                optional: true);
    }
}
