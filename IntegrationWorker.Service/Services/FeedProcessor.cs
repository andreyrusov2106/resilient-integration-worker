using IntegrationWorker.Service.Models;
using IntegrationWorker.Service.Repositories;
using IntegrationWorker.Service.Resilience;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Text.Json;

namespace IntegrationWorker.Service.Services;

public class FeedProcessor
{
    private readonly IFeedRepository _repository;
    private readonly ILogger<FeedProcessor> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public FeedProcessor(
        IFeedRepository repository,
        ILogger<FeedProcessor> logger)
    {
        _repository = repository;
        _logger = logger;

        // Создаём политики отказоустойчивости
        _retryPolicy = ResiliencePolicies.CreateRetryPolicy(logger);
        _circuitBreakerPolicy = ResiliencePolicies.CreateCircuitBreakerPolicy(logger);
    }

    public async Task InitializeDatabaseAsync()
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _repository.InitializeDatabaseAsync();
        });
    }

    public async Task ProcessFeedAsync()
    {
        // Комбинируем Circuit Breaker + Retry
        // Сначала проверяем Circuit Breaker, потом делаем Retry
        await _circuitBreakerPolicy.ExecuteAsync(async () =>
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var mockData = GenerateMockFeedData();

                    if (string.IsNullOrEmpty(mockData.ExternalId))
                    {
                        _logger.LogWarning("Invalid data: ExternalId is empty");
                        return;
                    }

                    // Импотентность: проверяем, не обрабатывали ли мы уже этот файл
                    var existing = await _repository.GetByExternalIdAsync(mockData.ExternalId);
                    if (existing != null)
                    {
                        _logger.LogInformation(
                            "Data with ExternalId {ExternalId} already exists, skipping",
                            mockData.ExternalId);
                        return;
                    }

                    await _repository.SaveAsync(mockData);
                    _logger.LogInformation("Successfully processed: {ExternalId}", mockData.ExternalId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing feed data");
                    throw; // Перебрасываем, чтобы Polly мог повторить
                }
            });
        });
    }

    private FeedData GenerateMockFeedData()
    {
        return new FeedData
        {
            Id = Guid.NewGuid(),
            ExternalId = $"EXT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            DataType = "Transaction",
            Payload = JsonSerializer.Serialize(new { Amount = 1000, Currency = "RUB" }),
            ReceivedAt = DateTime.UtcNow,
            IsProcessed = false
        };
    }
}