using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<Document> UploadDocumentAsync(int userId, int? projectId, int? taskId, string title, string? description, string category, Stream fileStream, string fileName, string contentType);
    Task<List<Document>> GetAccessibleDocumentsAsync(int userId, int? projectId = null, string? category = null, string? search = null);
    Task<Document?> GetDocumentByIdAsync(int documentId, int userId);
    Task<bool> DeleteDocumentAsync(int documentId, int userId, bool confirmDelete);
    Task<bool> ShareDocumentAsync(int documentId, int ownerUserId, int recipientUserId, string? message);
    Task<List<Document>> SearchAsync(int userId, string query);
    Task<List<DocumentAccessLog>> GetAuditLogAsync(int userId, int? documentId = null);
    Task<Stream?> DownloadDocumentAsync(int documentId, int userId);
    Task<bool> CanUserAccessDocumentAsync(int documentId, int userId);
}
