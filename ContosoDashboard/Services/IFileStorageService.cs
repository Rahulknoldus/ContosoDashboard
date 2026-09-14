namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativeDirectory);
    Task DeleteAsync(string relativePath);
    Task<Stream> DownloadAsync(string relativePath);
    string GetStorageRootPath();
}
