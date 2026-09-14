using System.ComponentModel.DataAnnotations;

namespace ContosoDashboard.Models;

public class ScanJobMessage
{
    [Key]
    public int ScanJobMessageId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    [MaxLength(255)]
    public string QueueName { get; set; } = "document-scan";

    [Required]
    [MaxLength(1000)]
    public string Payload { get; set; } = string.Empty;

    public ScanJobState State { get; set; } = ScanJobState.Queued;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAtUtc { get; set; }
}

public enum ScanJobState
{
    Queued,
    Processing,
    Completed,
    Failed
}
