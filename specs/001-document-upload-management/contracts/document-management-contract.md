# Document Management Contract

## Service Contracts

### IFileStorageService

```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativeDirectory);
    Task DeleteAsync(string relativePath);
    Task<Stream> DownloadAsync(string relativePath);
    Task<string> GetUrlAsync(string relativePath, TimeSpan expiration);
}
```

**Responsibilities**
- Store files outside the web root in the local training environment.
- Accept only validated file inputs.
- Return a relative path or storage key that can be persisted in the database.
- Support eventual cloud migration without altering UI or business flows.

### IDocumentService

```csharp
public interface IDocumentService
{
    Task<Document> UploadDocumentAsync(int userId, int? projectId, int? taskId, string title, string? description, string category, Stream fileStream, string fileName, string contentType);
    Task<List<Document>> GetAccessibleDocumentsAsync(int userId, int? projectId = null, string? category = null);
    Task<Document?> GetDocumentByIdAsync(int documentId, int userId);
    Task<bool> UpdateMetadataAsync(int documentId, int userId, string title, string? description, string category, string[]? tags);
    Task<bool> ReplaceFileAsync(int documentId, int userId, Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteDocumentAsync(int documentId, int userId, bool confirmDelete);
    Task<bool> ShareDocumentAsync(int documentId, int ownerUserId, int recipientUserId, string? message);
    Task<List<Document>> SearchAsync(int userId, string query);
    Task<List<DocumentAccessLog>> GetAuditLogAsync(int userId, int? documentId = null);
}
```

**Security expectations**
- Authorization checks must run on every request.
- Search results must be filtered to documents the current user can access.
- Any file download or file preview endpoint must validate both document permissions and the user’s identity.

## UI Contract Notes

The document pages and upload screens should follow the existing Blazor server pattern and use the user-specific service layer rather than directly accessing the DbContext from the UI. This keeps authorization rules centralized and consistent with the rest of the dashboard.
