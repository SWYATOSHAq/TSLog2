using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Links.JITLimport.Interfaces;

namespace TSLog.Links.JITLimport.Services;

public class FtpMonitorBackgroundService : BackgroundService
{
    private readonly IFtpMonitorService _ftpMonitorService;
    private readonly IExcelParsingService _excelParsingService;
    private readonly ILogger<FtpMonitorBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(3);

    public FtpMonitorBackgroundService(IFtpMonitorService ftpMonitorService, IExcelParsingService excelParsingService, ILogger<FtpMonitorBackgroundService> logger)
    {
        _ftpMonitorService = ftpMonitorService;
        _excelParsingService = excelParsingService;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var files = _ftpMonitorService.CheckForXlsFiles();
                await _ftpMonitorService.DownloadFilesAsync(files, stoppingToken);
                _ftpMonitorService.MoveRemoteFilesToArchive(FtpMonitorService.ARCHIEVE_PATH);

                string downloadedFilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedXlsFiles");
                _excelParsingService.ParseExcelFiles(downloadedFilesDir);
                _logger.LogInformation("Файл распарсен.");
                _ftpMonitorService.DeleteParsedFiles(downloadedFilesDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in FTP monitoring");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
