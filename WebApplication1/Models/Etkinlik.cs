using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Etkinlik
    {
        [Key]
        public int Id { get; set; } // Benzersiz Etkinlik ID

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(3000)]
        public string? DetailedProgram { get; set; }

        [Required]
        public DateTime Date { get; set; } // Tarih

        [Required]
        public TimeSpan Duration { get; set; } // Süre

        [Required, MaxLength(250)]
        public string Location { get; set; } = string.Empty;

        /// <summary>Null veya 0 ise sınırsız katılım anlamına gelir.</summary>
        [Range(0, 100000)]
        public int? MaxParticipants { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public EventStatus Status { get; set; } = EventStatus.Pending; // Varsayılan: Bekliyor


        [Required]
        public int CreatedByUserId { get; set; } // Etkinliği oluşturan kullanıcı ID

        [ForeignKey("CreatedByUserId")]
        public Kullanici CreatedByUser { get; set; } = null!;

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public bool IsDeleted { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }


        // Many-to-Many Relationship for Categories
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();

        // Participants Relationship
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();

        public ICollection<EventStaff> StaffMembers { get; set; } = new List<EventStaff>();

        public ICollection<AttendanceQrToken> AttendanceQrTokens { get; set; } = new List<AttendanceQrToken>();

        public ICollection<Mesaj> Messages { get; set; } = new List<Mesaj>(); // Mesajlar
    }

    // Etkinlik durumu enum
    public enum EventStatus
    {
        Pending,    // Onay bekliyor
        Approved,   // Onaylandı
        Rejected,   // Reddedildi
        Cancelled,
        Completed
    }
}
