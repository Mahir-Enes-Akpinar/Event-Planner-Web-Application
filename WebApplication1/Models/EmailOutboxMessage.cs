using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public sealed class EmailOutboxMessage
{
    public long Id { get; set; }
    [MaxLength(256)] public string Recipient { get; set; } = string.Empty;
    [MaxLength(200)] public string Subject { get; set; } = string.Empty;
    [MaxLength(4000)] public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime NextAttemptAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; set; }
    public int AttemptCount { get; set; }
    [MaxLength(1000)] public string? LastError { get; set; }
}
