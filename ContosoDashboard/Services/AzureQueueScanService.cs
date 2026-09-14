namespace ContosoDashboard.Services;

public class AzureQueueScanService : IScanQueueService
{
    private readonly ILogger<AzureQueueScanService> _logger;

    public AzureQueueScanService(ILogger<AzureQueueScanService> logger)
    {
        _logger = logger;
    }

    public Task EnqueueScanAsync(int documentId, string filePath, string fileName)
    {
        _logger.LogInformation("Queued document scan for document {DocumentId}. Path: {FilePath}. File: {FileName}", documentId, filePath, fileName);
        return Task.CompletedTask;
    }
}
