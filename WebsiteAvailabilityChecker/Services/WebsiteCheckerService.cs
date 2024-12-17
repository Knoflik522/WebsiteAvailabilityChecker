using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class WebsiteCheckerService : BackgroundService
{
    private readonly ILogger<WebsiteCheckerService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string UrlToCheck = "https://example.com"; // URL сторінки для перевірки
    private const string LogFilePath = "website_availability_log.txt";

    public WebsiteCheckerService(ILogger<WebsiteCheckerService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Website Checker Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckWebsiteAvailabilityAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Інтервал 10 хвилин
        }

        _logger.LogInformation("Website Checker Service is stopping.");
    }

    private async Task CheckWebsiteAvailabilityAsync(CancellationToken stoppingToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(UrlToCheck, stoppingToken);

            var status = response.IsSuccessStatusCode ? "Available" : "Unavailable";

            var logMessage = $"{DateTime.UtcNow}: {UrlToCheck} is {status} (Status Code: {response.StatusCode})";
            _logger.LogInformation(logMessage);

            // Запис у файл
            await File.AppendAllTextAsync(LogFilePath, logMessage + Environment.NewLine, stoppingToken);
        }
        catch (Exception ex)
        {
            var errorMessage = $"{DateTime.UtcNow}: Error while checking {UrlToCheck}. Exception: {ex.Message}";
            _logger.LogError(errorMessage);

            // Запис у файл
            await File.AppendAllTextAsync(LogFilePath, errorMessage + Environment.NewLine, stoppingToken);
        }
    }
}
