using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AttendanceQrToken
    {
        [Key]
        public long Id { get; set; }

        public int EtkinlikId { get; set; }
        public Etkinlik Etkinlik { get; set; } = null!;

        public int ParticipantId { get; set; }
        public Participant Participant { get; set; } = null!;

        [Required]
        [MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? UsedAtUtc { get; set; }
        public int? UsedByUserId { get; set; }
    }
}
