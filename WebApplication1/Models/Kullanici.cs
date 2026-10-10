using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class Kullanici : IdentityUser<int>
    {
        [MaxLength(250)]
        public string? Location { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime BirthDate { get; set; } // Doğum tarihi

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? ProfilePicture { get; set; }

        // Rol alanı (Admin: 1, Kullanıcı: 0)
        [Required]
        public RoleType Role { get; set; } = RoleType.User;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public DateTime? AnonymizeAtUtc { get; set; }
        public bool IsDeleted { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<Score> Scores { get; set; } = new List<Score>();

        // Many-to-Many Relationship for Interests
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
        public ICollection<Mesaj> SentMessages { get; set; } = new List<Mesaj>(); // Gönderilen mesajlar
        public ICollection<EventStaff> EventStaffAssignments { get; set; } = new List<EventStaff>();

    }
    public enum RoleType
    {
        Admin = 1,
        User = 0
    }
}
