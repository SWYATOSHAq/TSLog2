using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Links.JITLimport.Interfaces;
using TSLog.Links.JITLimport.Models;

namespace TSLog.Links.JITLimport.Services;

public class FtpMonitorBackgroundService : BackgroundService
{
    private readonly IFtpMonitorService _ftpMonitorService;
    private readonly IJsonParsingService _jsonParsingService;
    private readonly ILogger<FtpMonitorBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;

    public FtpMonitorBackgroundService(IFtpMonitorService ftpMonitorService, IJsonParsingService jsonParsingService, ILogger<FtpMonitorBackgroundService> logger, FtpMonitorConfig config)
    {
        _ftpMonitorService = ftpMonitorService;
        _jsonParsingService = jsonParsingService;
        _logger = logger;
        _checkInterval = TimeSpan.FromMinutes(config.CheckIntervalMinutes);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var files = _ftpMonitorService.CheckForJsonFiles();
                if (files.Count() == 0)
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                    continue; 
                }

                await _ftpMonitorService.DownloadFilesAsync(files, stoppingToken);
                _ftpMonitorService.MoveRemoteFilesToArchive(FtpMonitorService.ARCHIEVE_PATH);

                string downloadedFilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedJsonFiles");
                _jsonParsingService.ParseJsonFiles(downloadedFilesDir);
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
