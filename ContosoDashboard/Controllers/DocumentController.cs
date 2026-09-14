using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(
        [FromForm] int? projectId,
        [FromForm] int? taskId,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string category,
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "A file is required for upload." });
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new { message = "Document title is required." });
        }

        if (file.Length > 25 * 1024 * 1024)
        {
            return BadRequest(new { message = "The uploaded file exceeds the 25 MB maximum size." });
        }

        var extension = Path.GetExtension(file.FileName);
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".png", ".jpg", ".jpeg", ".gif"
        };

        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "The uploaded file type is not supported." });
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var document = await _documentService.UploadDocumentAsync(userId, projectId, taskId, title, description, category, stream, file.FileName, file.ContentType);

            return Ok(new
            {
                documentId = document.DocumentId,
                title = document.Title,
                status = document.Status,
                scanStatus = document.ScanStatus,
                fileName = document.FileName
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Document upload failed for user {UserId}.", userId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during document upload for user {UserId}.", userId);
            return StatusCode(500, new { message = "The document could not be uploaded at this time." });
        }
    }
}