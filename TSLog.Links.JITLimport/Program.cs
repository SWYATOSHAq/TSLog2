using Microsoft.Extensions.Configuration;
using System.Configuration;
using TSLog.Links.JITLimport.Services;


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

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
        }
    }
}