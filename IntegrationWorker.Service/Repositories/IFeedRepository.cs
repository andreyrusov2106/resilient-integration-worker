using IntegrationWorker.Service.Models;

namespace IntegrationWorker.Service.Repositories;

public interface IFeedRepository
{
    Task InitializeDatabaseAsync();
    Task SaveAsync(FeedData data);
    Task<FeedData?> GetByExternalIdAsync(string externalId);
    Task MarkAsProcessedAsync(Guid id);
}