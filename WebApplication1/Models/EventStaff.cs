using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class EventStaff
    {
        [Key]
        public int Id { get; set; }

        public int EtkinlikId { get; set; }
        public Etkinlik Etkinlik { get; set; } = null!;

        public int UserId { get; set; }
        public Kullanici User { get; set; } = null!;

        public EventStaffRole Role { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
    }

    public enum EventStaffRole
    {
        Assistant = 1,
        AttendanceOfficer = 2
    }
}
