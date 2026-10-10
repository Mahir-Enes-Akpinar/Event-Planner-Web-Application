using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Participant
    {
        [Key]
        public int Id { get; set; } // Benzersiz Katılım ID

        public int KullaniciId { get; set; } // Foreign Key (Kullanıcı)
        public Kullanici Kullanici { get; set; } = null!;

        public int EtkinlikId { get; set; } // Foreign Key (Etkinlik)
        public Etkinlik Etkinlik { get; set; } = null!;

        public DateTime ParticipationDate { get; set; } // Katılım Tarihi
        public ParticipationStatus Status { get; set; } = ParticipationStatus.Registered;
        public DateTime? AttendanceConfirmedAtUtc { get; set; }
        public int? AttendanceConfirmedByUserId { get; set; }

        public ICollection<AttendanceQrToken> AttendanceQrTokens { get; set; } = new List<AttendanceQrToken>();
    }

    public enum ParticipationStatus
    {
        Registered,
        Cancelled,
        Attended,
        NoShow
    }
}
