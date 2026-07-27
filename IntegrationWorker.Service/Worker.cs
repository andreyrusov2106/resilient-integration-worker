using IntegrationWorker.Service.Services;

namespace IntegrationWorker.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly FeedProcessor _feedProcessor;

    public Worker(ILogger<Worker> logger, FeedProcessor feedProcessor)
    {
        _logger = logger;
        _feedProcessor = feedProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker starting at: {Time}", DateTimeOffset.Now);

        await _feedProcessor.InitializeDatabaseAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Processing feed at: {Time}", DateTimeOffset.Now);
                await _feedProcessor.ProcessFeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker loop");
            }

            await Task.Delay(60000, stoppingToken);
        }
    }
}