using Microsoft.Extensions.Configuration;
using System.Configuration;
using TSLog.Links.JITLimport.Services;


namespace TSLog.Links.JITLimport
{
    internal class Program
    {
        static IConfiguration Configuration { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            Configuration = builder.Build();

            var ftpServer = Configuration["FtpSettings:ftpServer"];
            var ftpUsername = Configuration["FtpSettings:ftpUsername"];
            var ftpPassword = Configuration["FtpSettings:ftpPassword"];
            var remotePath = Configuration["FtpSettings:remotePath"];

            var ftpService = new FtpMonitorService(ftpServer, ftpUsername, ftpPassword, remotePath);

            var files = ftpService.CheckForXlsFiles();

            foreach (var file in files)
            {
                Console.WriteLine(file);
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