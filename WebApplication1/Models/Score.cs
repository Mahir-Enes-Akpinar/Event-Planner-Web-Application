using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Score
    {
        [Key]
        public int Id { get; set; } // Benzersiz Puan ID

        public int KullaniciId { get; set; } // Foreign Key (Kullanıcı)
        public Kullanici Kullanici { get; set; } = null!;

        public int Value { get; set; } // Kazanılan Puan

        public DateTime EarnedDate { get; set; } // Puan Kazanım Tarihi
        public ScoreReason Reason { get; set; }
        public int SourceId { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
    }

    public enum ScoreReason
    {
        EventApproved,
        Attendance,
        FirstAttendanceBonus,
        Reversal
    }
}
