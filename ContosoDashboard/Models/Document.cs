using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = "Other";

    public int? ProjectId { get; set; }

    public int? TaskId { get; set; }

    [Required]
    public int UploadedByUserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string StoredFilePath { get; set; } = string.Empty;

    [Required]
    public long FileSizeBytes { get; set; }

    [Required]
    [MaxLength(255)]
    public string MimeType { get; set; } = "application/octet-stream";

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Stored;

    public DocumentScanStatus ScanStatus { get; set; } = DocumentScanStatus.Pending;

    [ForeignKey("UploadedByUserId")]
    public virtual User Uploader { get; set; } = null!;

    [ForeignKey("ProjectId")]
    public virtual Project? Project { get; set; }

    [ForeignKey("TaskId")]
    public virtual TaskItem? Task { get; set; }

    public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
    public virtual ICollection<DocumentAccessLog> AccessLogs { get; set; } = new List<DocumentAccessLog>();
}

public enum DocumentStatus
{
    Stored,
    Archived,
    Deleted
}

public enum DocumentScanStatus
{
    Pending,
    Scanning,
    Clean,
    Quarantined,
    Failed
}
