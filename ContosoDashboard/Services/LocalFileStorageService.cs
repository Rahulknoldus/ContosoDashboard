namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRootPath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        var appDataPath = Path.Combine(environment.ContentRootPath, "AppData", "uploads");
        Directory.CreateDirectory(appDataPath);
        _storageRootPath = appDataPath;
    }

    public string GetStorageRootPath() => _storageRootPath;

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativeDirectory)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var safeRelativeDirectory = string.IsNullOrWhiteSpace(relativeDirectory) ? "general" : relativeDirectory.Trim('/','\\');
        var targetDirectory = Path.Combine(_storageRootPath, safeRelativeDirectory);
        Directory.CreateDirectory(targetDirectory);

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = $"{Guid.NewGuid():N}.bin";
        }

        var targetPath = Path.Combine(targetDirectory, $"{Guid.NewGuid():N}_{safeFileName}");
        await using var output = File.Create(targetPath);
        await fileStream.CopyToAsync(output);

        return Path.Combine(safeRelativeDirectory, Path.GetFileName(targetPath)).Replace('\\', '/');
    }

    public async Task DeleteAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var trimmedPath = relativePath.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_storageRootPath, trimmedPath);

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath));
        }
    }

    public async Task<Stream> DownloadAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path must be supplied.", nameof(relativePath));
        }

        var trimmedPath = relativePath.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_storageRootPath, trimmedPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Document file was not found in storage.", fullPath);
        }

        return await Task.FromResult<Stream>(File.OpenRead(fullPath));
    }
}
