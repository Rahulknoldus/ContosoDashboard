namespace ContosoDashboard.Services;

public interface IScanQueueService
{
    Task EnqueueScanAsync(int documentId, string filePath, string fileName);
}
