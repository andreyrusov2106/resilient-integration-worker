using IntegrationWorker.Service.Models;
using IntegrationWorker.Service.Repositories;
using IntegrationWorker.Service.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntegrationWorker.Tests;

public class FeedProcessorTests
{
    private readonly Mock<IFeedRepository> _mockRepository;
    private readonly Mock<ILogger<FeedProcessor>> _mockLogger;
    private readonly FeedProcessor _processor;

    public FeedProcessorTests()
    {
        _mockRepository = new Mock<IFeedRepository>();
        _mockLogger = new Mock<ILogger<FeedProcessor>>();

        _processor = new FeedProcessor(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessFeedAsync_WhenDataIsValid_ShouldSaveToRepository()
    {
        _mockRepository.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>()))
                       .ReturnsAsync((FeedData?)null);

        await _processor.ProcessFeedAsync();

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<FeedData>()), Times.Once);
    }

    [Fact]
    public async Task ProcessFeedAsync_WhenDataAlreadyExists_ShouldSkipSaving()
    {
        var existingData = new FeedData
        {
            Id = Guid.NewGuid(),
            ExternalId = "EXT-EXISTING",
            DataType = "Test",
            Payload = "{}",
            ReceivedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        _mockRepository.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>()))
                       .ReturnsAsync(existingData);

        await _processor.ProcessFeedAsync();

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<FeedData>()), Times.Never);
    }

    [Fact]
    public async Task ProcessFeedAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        _mockRepository.Setup(r => r.GetByExternalIdAsync(It.IsAny<string>()))
                       .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => _processor.ProcessFeedAsync());
    }
}