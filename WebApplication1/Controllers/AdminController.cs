using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Authorize(Policy = "AdminMfa")]
    public class AdminController : Controller
    {

        private readonly Context _context;
        private readonly IImageStorage _imageStorage;
        private readonly UserManager<Kullanici> _userManager;

        public AdminController(Context context, IImageStorage imageStorage, UserManager<Kullanici> userManager)
        {
            _context = context;
            _imageStorage = imageStorage;
            _userManager = userManager;
        }

        // ADMİN ANA SAYFA
        public IActionResult Index()
        {
            // Bekleyen etkinlikleri al
            var pendingEvents = _context.Etkinlikler.Where(e => e.Status == EventStatus.Pending).ToList();
            ViewData["PendingEvents"] = pendingEvents;

            ViewData["Title"] = "Admin Dashboard";
            return View();
        }


        //ONAYLAMA
        [HttpPost]
        public IActionResult ApproveEvent(int eventId)
        {
            // Etkinliği ID'ye göre bul
            var eventItem = _context.Etkinlikler.FirstOrDefault(e => e.Id == eventId);
            if (eventItem != null)
            {
                if (eventItem.Status != EventStatus.Pending)
                    return Conflict("Yalnızca bekleyen etkinlik onaylanabilir.");
                eventItem.Status = EventStatus.Approved; // Durumu "Onaylandı" olarak güncelle
                eventItem.UpdatedAtUtc = DateTime.UtcNow;
                var existingScore = _context.Puanlar.FirstOrDefault(score =>
                    score.KullaniciId == eventItem.CreatedByUserId &&
                    score.Reason == ScoreReason.EventApproved && score.SourceId == eventItem.Id);
                if (existingScore is null)
                {
                    _context.Puanlar.Add(new Score
                    {
                        KullaniciId = eventItem.CreatedByUserId,
                        Value = 15,
                        Reason = ScoreReason.EventApproved,
                        SourceId = eventItem.Id,
                        EarnedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    existingScore.RevokedAtUtc = null;
                }
                Audit("EventApproved", nameof(Etkinlik), eventItem.Id);
                _context.SaveChanges(); // Değişiklikleri veritabanına kaydet
            }

            // Onay bekleyen etkinlikler sayfasına geri dön
            return RedirectToAction("Index");
        }


        //REDDETME
        [HttpPost]
        public IActionResult RejectEvent(int eventId, string rejectionReason)
        {
            // Etkinliği ID'ye göre bul
            var eventItem = _context.Etkinlikler.FirstOrDefault(e => e.Id == eventId);
            if (eventItem != null)
            {
                if (eventItem.Status != EventStatus.Pending)
                    return Conflict("Yalnızca bekleyen etkinlik reddedilebilir.");
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    TempData["ErrorMessage"] = "Ret gerekçesi zorunludur.";
                    return RedirectToAction("Index");
                }
                eventItem.Status = EventStatus.Rejected;
                eventItem.RejectionReason = rejectionReason.Trim();
                eventItem.UpdatedAtUtc = DateTime.UtcNow;
                Audit("EventRejected", nameof(Etkinlik), eventItem.Id);
                _context.SaveChanges(); // Değişiklikleri veritabanına kaydet 
            }

            // Onay bekleyen etkinlikler sayfasına geri dön
            return RedirectToAction("Index");
        }


        //ADMİN USER KISMI

        public IActionResult Users()
        {
            // Veritabanından kullanıcı listesini çek
            var users = _context.Kullanicilar.ToList();

            ViewData["Title"] = "Kullanıcı Yönetimi";
            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = _context.Kullanicilar.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.DeletedAtUtc = DateTime.UtcNow;
                user.AnonymizeAtUtc = DateTime.UtcNow.AddDays(30);
                user.SecurityStamp = Guid.NewGuid().ToString();
                Audit("UserDeletionScheduled", nameof(Kullanici), user.Id);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Users");
        }


//*------------------------------------------- Kullanıcı Güncellme İşlemi-------------------------------------------------------------------------------------------------*//
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Kullanicilar.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(Kullanici updatedUser)
        {
            var user = _context.Kullanicilar.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = updatedUser.UserName;
            user.Email = updatedUser.Email;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.BirthDate = updatedUser.BirthDate;
            user.Gender = updatedUser.Gender;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Location = updatedUser.Location;
            user.Role = updatedUser.Role;
            user.UpdatedAtUtc = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(" ", updateResult.Errors.Select(error => error.Description));
                return RedirectToAction("EditUser", new { id = user.Id });
            }

            var desiredRole = updatedUser.Role == RoleType.Admin ? "Admin" : "User";
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles.Where(role => role is "Admin" or "User"));
            await _userManager.AddToRoleAsync(user, desiredRole);

            TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            return RedirectToAction("Users");
        }
//*--------------------------------------Etkinlikler--------------------------------------------------------------------------------------------*//
        public IActionResult Events()
        {
            // Onaylanmış etkinlikleri getir
            var etkinlikler = _context.Etkinlikler
                              .Include(e => e.EventCategories)
                              .ThenInclude(ec => ec.Kategori)
                              .Where(e => e.Status == EventStatus.Approved) // Onaylanmış etkinlikler
                              .ToList();
            return View(etkinlikler);
        }

        public IActionResult Categories() => View(_context.Kategoriler.OrderBy(item => item.Name).ToList());

        [HttpPost]
        public IActionResult CreateCategory(string name)
        {
            var normalized = name?.Trim();
            if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 50)
            {
                TempData["ErrorMessage"] = "Kategori adı 1-50 karakter olmalıdır.";
                return RedirectToAction(nameof(Categories));
            }
            if (_context.Kategoriler.Any(item => item.Name == normalized))
            {
                TempData["ErrorMessage"] = "Bu kategori zaten mevcut.";
                return RedirectToAction(nameof(Categories));
            }
            _context.Kategoriler.Add(new Kategori { Name = normalized });
            _context.SaveChanges();
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        public IActionResult ArchiveCategory(int id)
        {
            var category = _context.Kategoriler.FirstOrDefault(item => item.Id == id);
            if (category is null) return NotFound();
            category.IsArchived = true;
            _context.SaveChanges();
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var eventItem = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
            if (eventItem != null)
            {
                eventItem.IsDeleted = true;
                eventItem.DeletedAtUtc = DateTime.UtcNow;
                eventItem.Status = EventStatus.Cancelled;
                Audit("EventSoftDeleted", nameof(Etkinlik), eventItem.Id);
                _context.SaveChanges();
            }
            return RedirectToAction("Events");
        }





        //*------------------------------------------- Etkinlik Güncellme İşlemi-------------------------------------------------------------------------------------------------*//
        [HttpGet]
        public IActionResult EditEvent(int id)
        {
            var etkinlik = _context.Etkinlikler.Include(e => e.EventCategories).FirstOrDefault(e => e.Id == id);
            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("Index");
            }

            // Kategorileri yükle
            ViewBag.Categories = _context.Kategoriler.Where(item => !item.IsArchived).ToList();

            return View(etkinlik);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(Etkinlik model, IFormFile Image, int selectedCategoryId, int DurationHours, int DurationMinutes)
        {
            var etkinlik = _context.Etkinlikler.Include(e => e.EventCategories).FirstOrDefault(e => e.Id == model.Id);
            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("Index");
            }

            if (model.RowVersion is null || model.RowVersion.Length == 0)
            {
                TempData["ErrorMessage"] = "Etkinlik sürüm bilgisi eksik. Sayfayı yenileyip tekrar deneyin.";
                return RedirectToAction("EditEvent", new { id = model.Id });
            }

            _context.Entry(etkinlik).Property(item => item.RowVersion).OriginalValue = model.RowVersion;

            if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length > 100 ||
                model.Description?.Length > 500 || DurationHours is < 0 or > 23 ||
                DurationMinutes is < 0 or > 59 || (DurationHours == 0 && DurationMinutes == 0) ||
                string.IsNullOrWhiteSpace(model.Location) || model.Location.Trim().Length > 250 ||
                !_context.Kategoriler.Any(item => item.Id == selectedCategoryId && !item.IsArchived))
            {
                TempData["ErrorMessage"] = "Etkinlik bilgileri geçersiz.";
                return RedirectToAction("EditEvent", new { id = model.Id });
            }

            // Etkinlik bilgilerini güncelle
            etkinlik.Name = model.Name.Trim();
            etkinlik.Description = model.Description?.Trim();
            etkinlik.Date = model.Date;
            etkinlik.Duration = new TimeSpan(DurationHours, DurationMinutes, 0);
            etkinlik.Location = model.Location.Trim();
            etkinlik.UpdatedAtUtc = DateTime.UtcNow;

            // Resim dosyasını güncelle
            if (Image != null && Image.Length > 0)
            {
                try
                {
                    etkinlik.ImageUrl = await _imageStorage.StoreAsync(Image, 5 * 1024 * 1024);
                }
                catch (InvalidDataException exception)
                {
                    TempData["ErrorMessage"] = exception.Message;
                    return RedirectToAction("EditEvent", new { id = model.Id });
                }
            }

            // Kategori güncellemesi
            etkinlik.EventCategories.Clear();
            etkinlik.EventCategories.Add(new EventCategory
            {
                EtkinlikId = etkinlik.Id,
                KategoriId = selectedCategoryId
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Etkinlik başka bir kullanıcı tarafından değiştirildi. Güncel bilgileri kontrol edin.";
                return RedirectToAction("EditEvent", new { id = model.Id });
            }

            TempData["SuccessMessage"] = "Etkinlik başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        private void Audit(string action, string entityType, object entityId) =>
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
                Action = action,
                EntityType = entityType,
                EntityId = entityId.ToString() ?? string.Empty,
                CorrelationId = HttpContext.TraceIdentifier
            });


    }
}
