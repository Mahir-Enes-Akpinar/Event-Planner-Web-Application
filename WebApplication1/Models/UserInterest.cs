using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

public class UserInterest
{
    public int KullaniciId { get; set; } // Foreign Key (Kullanıcı)
    public Kullanici Kullanici { get; set; } = null!;

    public int KategoriId { get; set; } // Foreign Key (Kategori)
    public Kategori Kategori { get; set; } = null!;
}
