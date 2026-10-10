using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Kategori
    {
        [Key]
        public int Id { get; set; } // Benzersiz Kategori ID

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public bool IsArchived { get; set; }

        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>(); // Kullanıcı İlişkisi
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>(); // Etkinlik İlişkisi
    }
}
