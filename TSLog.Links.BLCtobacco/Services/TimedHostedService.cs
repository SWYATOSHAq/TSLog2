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
    private readonly IDataProcessor _dataProcessor;

    public TimedHostedService(ILogger<TimedHostedService> logger, IOptions<TimeHostConfig> config, IHttpClientService httpClientService, IDataProcessor dataProcessor)
    {
        _checkInterval = TimeSpan.FromMinutes(config.Value.CheckIntervalMinutes);
        _logger = logger;
        _httpClientService = httpClientService;
        _dataProcessor = dataProcessor;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var responce = await _httpClientService.SendGetRequestAsync("https://hub.sns.ru/odata/habapi/ordsAttrsOndateByStsSet?$format=json&$filter=apiClass eq 'MM' and hub eq 'H05B' and password eq '7P2SGSA5' and orderDate eq datetime'2024-05-09T00:00:00' and orderStatus eq ''");
                //var data = _dataProcessor.ParseResponse(responce);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the GET request.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
