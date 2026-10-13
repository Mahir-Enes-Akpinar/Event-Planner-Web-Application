using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public sealed class StoredImage
{
    [Key]
    [MaxLength(50)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
