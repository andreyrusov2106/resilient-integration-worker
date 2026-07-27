namespace IntegrationWorker.Service.Models;

public class FeedData
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool IsProcessed { get; set; }
}