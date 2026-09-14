using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IScanQueueService _scanQueueService;
    private readonly ILogger<DocumentService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".png", ".jpg", ".jpeg", ".gif"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        IScanQueueService scanQueueService,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _scanQueueService = scanQueueService;
        _logger = logger;
    }

    public async Task<Document> UploadDocumentAsync(int userId, int? projectId, int? taskId, string title, string? description, string category, Stream fileStream, string fileName, string contentType)
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Document title is required.", nameof(title));
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("The uploaded file type is not supported.");
        }

        await using var bufferedFile = new MemoryStream();
        await fileStream.CopyToAsync(bufferedFile);
        if (bufferedFile.Length == 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        if (bufferedFile.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("The uploaded file exceeds the 25 MB maximum size.");
        }

        bufferedFile.Position = 0;

        var storageDirectory = projectId.HasValue ? $"projects/{projectId.Value}" : "personal";
        var storedRelativePath = await _fileStorageService.UploadAsync(bufferedFile, fileName, contentType, storageDirectory);

        var document = new Document
        {
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? "Other" : category.Trim(),
            ProjectId = projectId,
            TaskId = taskId,
            UploadedByUserId = userId,
            FileName = Path.GetFileName(fileName),
            StoredFilePath = storedRelativePath,
            FileSizeBytes = bufferedFile.Length,
            MimeType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            UploadedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Status = DocumentStatus.Stored,
            ScanStatus = DocumentScanStatus.Pending
        };

        try
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _fileStorageService.DeleteAsync(storedRelativePath);
            throw;
        }

        await _scanQueueService.EnqueueScanAsync(document.DocumentId, document.StoredFilePath, document.FileName);
        await LogAccessAsync(document.DocumentId, userId, "Upload", $"Uploaded file '{document.FileName}'");

        return document;
    }

    public async Task<List<Document>> GetAccessibleDocumentsAsync(int userId, int? projectId = null, string? category = null, string? search = null)
    {
        var query = _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .Where(d => !d.IsDeleted && (d.UploadedByUserId == userId || (d.ProjectId.HasValue && _context.ProjectMembers.Any(pm => pm.ProjectId == d.ProjectId.Value && pm.UserId == userId)) || d.Shares.Any(s => s.IsActive && s.SharedWithUserId == userId)));

        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(d => d.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            query = query.Where(d => d.Title.Contains(normalized) || (d.Description != null && d.Description.Contains(normalized)) || d.FileName.Contains(normalized));
        }

        return await query
            .OrderByDescending(d => d.UploadedAtUtc)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.Uploader)
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null)
        {
            return null;
        }

        if (!await CanUserAccessDocumentAsync(documentId, userId))
        {
            return null;
        }

        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int userId, bool confirmDelete)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null)
        {
            return false;
        }

        if (document.UploadedByUserId != userId && !await IsProjectManagerOrMemberAsync(document.ProjectId, userId))
        {
            return false;
        }

        if (!confirmDelete)
        {
            return false;
        }

        document.IsDeleted = true;
        document.Status = DocumentStatus.Deleted;
        document.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await _fileStorageService.DeleteAsync(document.StoredFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete document file for {DocumentId} during delete workflow.", documentId);
        }

        await _context.SaveChangesAsync();
        await LogAccessAsync(documentId, userId, "Delete", "Deleted document record");
        return true;
    }

    public async Task<bool> ShareDocumentAsync(int documentId, int ownerUserId, int recipientUserId, string? message)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);
        if (document == null || document.UploadedByUserId != ownerUserId)
        {
            return false;
        }

        if (recipientUserId == ownerUserId)
        {
            return false;
        }

        var existingShare = await _context.DocumentShares
            .FirstOrDefaultAsync(s => s.DocumentId == documentId && s.SharedWithUserId == recipientUserId && s.IsActive);

        if (existingShare != null)
        {
            return false;
        }

        _context.DocumentShares.Add(new DocumentShare
        {
            DocumentId = documentId,
            SharedWithUserId = recipientUserId,
            SharedByUserId = ownerUserId,
            Message = message,
            SharedAtUtc = DateTime.UtcNow,
            IsActive = true
        });

        await _context.SaveChangesAsync();
        await LogAccessAsync(documentId, ownerUserId, "Share", $"Shared with user {recipientUserId}");
        return true;
    }

    public async Task<List<Document>> SearchAsync(int userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAccessibleDocumentsAsync(userId);
        }

        return await GetAccessibleDocumentsAsync(userId, search: query);
    }

    public async Task<List<DocumentAccessLog>> GetAuditLogAsync(int userId, int? documentId = null)
    {
        var permittedDocumentIds = await _context.Documents
            .Where(d => !d.IsDeleted)
            .Where(d => d.UploadedByUserId == userId || (d.ProjectId.HasValue && _context.ProjectMembers.Any(pm => pm.ProjectId == d.ProjectId.Value && pm.UserId == userId)) || d.Shares.Any(s => s.IsActive && s.SharedWithUserId == userId))
            .Select(d => d.DocumentId)
            .ToListAsync();

        if (!permittedDocumentIds.Any())
        {
            return new List<DocumentAccessLog>();
        }

        var query = _context.DocumentAccessLogs
            .Include(dal => dal.User)
            .Where(dal => permittedDocumentIds.Contains(dal.DocumentId));

        if (documentId.HasValue)
        {
            query = query.Where(dal => dal.DocumentId == documentId.Value);
        }

        return await query
            .OrderByDescending(dal => dal.ActionAtUtc)
            .ToListAsync();
    }

    public async Task<Stream?> DownloadDocumentAsync(int documentId, int userId)
    {
        var document = await GetDocumentByIdAsync(documentId, userId);
        if (document == null)
        {
            return null;
        }

        var fileStream = await _fileStorageService.DownloadAsync(document.StoredFilePath);
        await LogAccessAsync(documentId, userId, "Download", $"Downloaded file '{document.FileName}'");
        return fileStream;
    }

    public async Task<bool> CanUserAccessDocumentAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null)
        {
            return false;
        }

        if (document.UploadedByUserId == userId)
        {
            return true;
        }

        if (document.ProjectId.HasValue)
        {
            var isProjectParticipant = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == document.ProjectId.Value && pm.UserId == userId);

            if (isProjectParticipant)
            {
                return true;
            }
        }

        return document.Shares.Any(s => s.IsActive && s.SharedWithUserId == userId);
    }

    private async Task LogAccessAsync(int documentId, int userId, string actionType, string details)
    {
        _context.DocumentAccessLogs.Add(new DocumentAccessLog
        {
            DocumentId = documentId,
            UserId = userId,
            ActionType = actionType,
            ActionAtUtc = DateTime.UtcNow,
            Details = details
        });

        await _context.SaveChangesAsync();
    }

    private async Task<bool> IsProjectManagerOrMemberAsync(int? projectId, int userId)
    {
        if (!projectId.HasValue)
        {
            return false;
        }

        return await _context.Projects
            .AnyAsync(p => p.ProjectId == projectId.Value && (p.ProjectManagerId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId)));
    }
}