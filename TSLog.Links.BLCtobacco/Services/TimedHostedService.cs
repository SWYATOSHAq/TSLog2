using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSLog.Links.BLCtobacco.Interfaces;
using TSLog.Links.BLCtobacco.Models;

namespace TSLog.Links.BLCtobacco.Services;

internal class TimedHostedService : BackgroundService
{
    private readonly ILogger<TimedHostedService> _logger;
    private readonly IHttpClientService _httpClientService;
    private readonly TimeSpan _checkInterval;

    public TimedHostedService(ILogger<TimedHostedService> logger, IOptions<TimeHostConfig> config, IHttpClientService httpClientService)
    {
        _checkInterval = TimeSpan.FromMinutes(config.Value.CheckIntervalMinutes);
        _logger = logger;
        _httpClientService = httpClientService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _httpClientService.SendGetRequestAsync("https://www.google.com/");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in FTP monitoring");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
