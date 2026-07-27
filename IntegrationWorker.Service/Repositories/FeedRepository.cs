using Dapper;
using IntegrationWorker.Service.Models;
using Npgsql;

namespace IntegrationWorker.Service.Repositories;

public class FeedRepository : IFeedRepository
{
    private readonly string _connectionString;
    private readonly ILogger<FeedRepository> _logger;

    public FeedRepository(IConfiguration configuration, ILogger<FeedRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("Postgres")!;
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS feed_data (
                id UUID PRIMARY KEY,
                external_id VARCHAR(100) NOT NULL UNIQUE,
                data_type VARCHAR(50) NOT NULL,
                payload TEXT NOT NULL,
                received_at TIMESTAMP NOT NULL,
                is_processed BOOLEAN NOT NULL DEFAULT FALSE
            );
            
            CREATE INDEX IF NOT EXISTS idx_feed_external_id 
            ON feed_data(external_id);";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(createTableSql);
        _logger.LogInformation("Database initialized");
    }

    public async Task SaveAsync(FeedData data)
    {
        const string insertSql = @"
            INSERT INTO feed_data (id, external_id, data_type, payload, received_at, is_processed)
            VALUES (@Id, @ExternalId, @DataType, @Payload, @ReceivedAt, @IsProcessed)";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(insertSql, data);
    }

    public async Task<FeedData?> GetByExternalIdAsync(string externalId)
    {
        const string selectSql = @"
            SELECT * FROM feed_data WHERE external_id = @ExternalId";

        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<FeedData>(selectSql, new { ExternalId = externalId });
    }

    public async Task MarkAsProcessedAsync(Guid id)
    {
        const string updateSql = @"
            UPDATE feed_data SET is_processed = TRUE WHERE id = @Id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(updateSql, new { Id = id });
    }
}