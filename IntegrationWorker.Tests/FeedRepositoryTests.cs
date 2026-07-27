using IntegrationWorker.Service.Models;
using IntegrationWorker.Service.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntegrationWorker.Tests;

public class FeedRepositoryTests
{
    private readonly IFeedRepository _repository;

    public FeedRepositoryTests()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetConnectionString("Postgres"))
                  .Returns("Host=localhost;Port=5432;Database=integration_db;Username=postgres;Password=postgres");

        var mockLogger = new Mock<ILogger<FeedRepository>>();

        _repository = new FeedRepository(mockConfig.Object, mockLogger.Object);
    }

    [Fact]
    public async Task SaveAsync_ShouldSaveDataToDatabase()
    {
        await _repository.InitializeDatabaseAsync();

        var testData = new FeedData
        {
            Id = Guid.NewGuid(),
            ExternalId = "TEST-001",
            DataType = "TestType",
            Payload = "{\"test\": \"data\"}",
            ReceivedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        await _repository.SaveAsync(testData);

        var retrieved = await _repository.GetByExternalIdAsync("TEST-001");

        Assert.NotNull(retrieved);
        Assert.Equal("TEST-001", retrieved.ExternalId);
        Assert.Equal("TestType", retrieved.DataType);
    }

    [Fact]
    public async Task GetByExternalIdAsync_WhenNotFound_ShouldReturnNull()
    {
        await _repository.InitializeDatabaseAsync();

        var result = await _repository.GetByExternalIdAsync("NON-EXISTENT-ID");

        Assert.Null(result);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldUpdateIsProcessedFlag()
    {
        await _repository.InitializeDatabaseAsync();

        var testData = new FeedData
        {
            Id = Guid.NewGuid(),
            ExternalId = "TEST-002",
            DataType = "TestType",
            Payload = "{}",
            ReceivedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        await _repository.SaveAsync(testData);

        await _repository.MarkAsProcessedAsync(testData.Id);

        var retrieved = await _repository.GetByExternalIdAsync("TEST-002");
        Assert.NotNull(retrieved);
        Assert.True(retrieved.IsProcessed);
    }
}