using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public sealed class AuditLog
{
    public long Id { get; set; }
    public int? ActorUserId { get; set; }
    [MaxLength(80)] public string Action { get; set; } = string.Empty;
    [MaxLength(80)] public string EntityType { get; set; } = string.Empty;
    [MaxLength(80)] public string EntityId { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CorrelationId { get; set; }
}
