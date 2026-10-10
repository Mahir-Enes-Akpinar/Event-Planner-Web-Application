using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Mesaj
    {
        [Key]
        public int Id { get; set; } // Benzersiz Mesaj ID

        public int SenderId { get; set; } // Foreign Key (Gönderen Kullanıcı)
        public Kullanici Sender { get; set; } = null!;

        public int EtkinlikId { get; set; } // Foreign Key (Etkinlik))
        public Etkinlik Etkinlik { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime SentDate { get; set; } // Gönderim tarihi

        public bool IsPinned { get; set; }
        public DateTime? PinnedAtUtc { get; set; }
        public int? PinnedByUserId { get; set; }
    }
}
