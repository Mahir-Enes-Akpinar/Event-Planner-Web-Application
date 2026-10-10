using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly Context _context;
        private readonly IImageStorage _imageStorage;

        public EventController(Context context, IImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        [HttpGet]
        public IActionResult AddEvent()
        {

            var userId = CurrentUserId();
            var userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";

            ViewData["UserProfilePicture"] = HttpContext.Session.GetString("UserProfilePicture") ?? "~/images/profile.png";

            ViewData["UserName"] = userName;
            ViewData["UserId"] = userId;

            // Kategorileri ViewBag ile gönder
            ViewBag.Categories = _context.Kategoriler.Where(item => !item.IsArchived).ToList();
            return View();
        }



//*--------------------------------------------------Yeni Etkinlik Ekleme -----------------------------------------------------------------------------------*//
        [HttpPost]
        public async Task<IActionResult> AddEvent(string Name, string Description, string DetailedProgram, DateTime Date, int DurationHours, int DurationMinutes, string Address, double Latitude, double Longitude, int? MaxParticipants, int selectedCategoryId, IFormFile Image)
        {
            // Kullanıcı ID'sini oturumdan al
            var userId = CurrentUserId();

            if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > 100 ||
                Description?.Length > 500 || Date <= DateTime.Now ||
                DetailedProgram?.Length > 3000 ||
                DurationHours is < 0 or > 23 || DurationMinutes is < 0 or > 59 ||
                (DurationHours == 0 && DurationMinutes == 0) ||
                MaxParticipants is < 0 or > 100000 ||
                string.IsNullOrWhiteSpace(Address) || Address.Trim().Length > 250 ||
                Latitude is < -90 or > 90 || Longitude is < -180 or > 180)
            {
                TempData["ErrorMessage"] = "Etkinlik bilgileri geçersiz. Tarih, süre, konum ve metin sınırlarını kontrol edin.";
                return RedirectToAction("AddEvent");
            }

            // Seçilen kategori kontrolü
            var selectedCategory = _context.Kategoriler.FirstOrDefault(c => c.Id == selectedCategoryId && !c.IsArchived);
            if (selectedCategory == null)
            {
                TempData["ErrorMessage"] = "Geçerli bir kategori seçin.";
                return RedirectToAction("AddEvent");
            }
            // Süreyi TimeSpan'e dönüştür
            var duration = new TimeSpan(DurationHours, DurationMinutes, 0);
            if (duration.TotalHours >= 24)
            {
                TempData["ErrorMessage"] = "Süre 24 saatten büyük olamaz.";
                return RedirectToAction("AddEvent");
            }


            // Resim dosyasını kaydet
            string? imageUrl = null;
            if (Image != null && Image.Length > 0)
            {
                try
                {
                    imageUrl = await _imageStorage.StoreAsync(Image, 5 * 1024 * 1024);
                }
                catch (InvalidDataException exception)
                {
                    TempData["ErrorMessage"] = exception.Message;
                    return RedirectToAction("AddEvent");
                }
            }

            // Yeni etkinlik oluştur
            var newEvent = new Etkinlik
            {
                Name = Name.Trim(),
                Description = Description?.Trim(),
                DetailedProgram = DetailedProgram?.Trim(),
                Date = Date,
                Duration = duration,
                Location = Address.Trim(),
                MaxParticipants = MaxParticipants is > 0 ? MaxParticipants : null,
                Latitude = Latitude,
                Longitude = Longitude,
                ImageUrl = imageUrl, // Resim URL'sini ata
                Status = EventStatus.Pending, // Varsayılan durum
                CreatedByUserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Kategoriyi ilişkilendir
            newEvent.EventCategories.Add(new EventCategory
            {
                Etkinlik = newEvent,
                Kategori = selectedCategory
            });

            // Veritabanına kaydet
            _context.Etkinlikler.Add(newEvent);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Etkinlik başarıyla eklendi ve onay bekliyor.";
            return RedirectToAction("Index", "User");
        }

 //*--------------------------------------------------Map Üzerinden Etkinlikleri Gösterme-----------------------------------------------------------------------------------*//
        public IActionResult Map()
        {
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Misafir Kullanıcı";
            ViewData["UserProfilePicture"] = HttpContext.Session.GetString("UserProfilePicture") ?? "~/images/profile.png";

            // Tüm onaylanmış etkinlikleri al
            var etkinlikler = _context.Etkinlikler
                .Where(e => e.Status == EventStatus.Approved)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Location,
                    e.Latitude,
                    e.Longitude,
                    e.Date,
                    e.Duration,
                    e.Description,
                    e.ImageUrl
                })
                .ToList();

            // JSON olarak View'e gönder
            ViewData["EtkinliklerJson"] = System.Text.Json.JsonSerializer.Serialize(etkinlikler);
            return View();
        }



//*--------------------------------------------------Etkinlik Detay ve Mesajlasşma-----------------------------------------------------------------------------------*//

        public IActionResult Details(int id)
        {
            


            var etkinlik = _context.Etkinlikler
                                    .Include(e => e.CreatedByUser)
                                    .Include(e => e.Messages)
                                    .ThenInclude(m => m.Sender)
                                    .Include(e => e.Participants)
                                    .ThenInclude(p => p.Kullanici)
                                    .FirstOrDefault(e => e.Id == id &&
                                        (e.Status == EventStatus.Approved || e.CreatedByUserId == CurrentUserId() || User.IsInRole("Admin")));

            if (etkinlik == null)
            {
                return NotFound();
            }


            return View(etkinlik);
        }

        public IActionResult Community(int id)
        {
            var userId = CurrentUserId();
            var etkinlik = _context.Etkinlikler
                .Include(e => e.CreatedByUser)
                .Include(e => e.Messages)
                .ThenInclude(m => m.Sender)
                .Include(e => e.Participants)
                .ThenInclude(p => p.Kullanici)
                .Include(e => e.StaffMembers)
                .ThenInclude(s => s.User)
                .FirstOrDefault(e => e.Id == id &&
                    (e.Status == EventStatus.Approved || e.CreatedByUserId == userId || User.IsInRole("Admin")));

            if (etkinlik is null) return NotFound();
            if (!CanAccessCommunity(etkinlik, userId)) return Forbid();

            return View(etkinlik);
        }


        // Yorum Ekleme
        [HttpPost]
        public IActionResult AddComment(int etkinlikId, string content, bool fromCommunity = false)
        {
            // Kullanıcı oturum kontrolü
            var userId = CurrentUserId();

            var etkinlik = _context.Etkinlikler
                .Include(item => item.Participants)
                .FirstOrDefault(item => item.Id == etkinlikId && item.Status == EventStatus.Approved);

            if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > 1000 || etkinlik is null)
                return BadRequest();

            if (!CanAccessCommunity(etkinlik, userId))
                return Forbid();

            // Yeni yorum oluşturma
            var yeniYorum = new Mesaj
            {
                EtkinlikId = etkinlikId,
                SenderId = userId,
                Content = content.Trim(),
                SentDate = DateTime.UtcNow
            };

            _context.Mesajlar.Add(yeniYorum);
            _context.SaveChanges();

            return RedirectToAction(fromCommunity ? "Community" : "Details", new { id = etkinlikId });
        }

        [HttpPost]
        public IActionResult TogglePinnedComment(int messageId)
        {
            var message = _context.Mesajlar
                .Include(item => item.Etkinlik)
                .ThenInclude(item => item.Participants)
                .FirstOrDefault(item => item.Id == messageId);
            if (message is null) return NotFound();

            var currentUserId = CurrentUserId();
            if (!CanAccessCommunity(message.Etkinlik, currentUserId))
                return Forbid();

            if (!CanManageCommunity(message.Etkinlik, currentUserId))
                return Forbid();

            message.IsPinned = !message.IsPinned;
            message.PinnedAtUtc = message.IsPinned ? DateTime.UtcNow : null;
            message.PinnedByUserId = message.IsPinned ? currentUserId : null;
            Audit(message.IsPinned ? "CommentPinned" : "CommentUnpinned", nameof(Mesaj), message.Id);
            _context.SaveChanges();

            return RedirectToAction("Community", new { id = message.EtkinlikId });
        }

        [HttpPost]
        public IActionResult UpdateCommunityProgram(int eventId, string detailedProgram)
        {
            var eventItem = _context.Etkinlikler
                .Include(item => item.Participants)
                .FirstOrDefault(item => item.Id == eventId);
            if (eventItem is null) return NotFound();
            if (!CanManageCommunity(eventItem, CurrentUserId())) return Forbid();
            if (detailedProgram?.Length > 3000)
            {
                TempData["ErrorMessage"] = "Detaylı program en fazla 3000 karakter olabilir.";
                return RedirectToAction("Community", new { id = eventId });
            }

            eventItem.DetailedProgram = detailedProgram?.Trim();
            eventItem.UpdatedAtUtc = DateTime.UtcNow;
            Audit("CommunityProgramUpdated", nameof(Etkinlik), eventItem.Id);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Detaylı program güncellendi.";
            return RedirectToAction("Community", new { id = eventId });
        }

        [HttpPost]
        public IActionResult AssignEventStaff(int eventId, int userId, EventStaffRole role)
        {
            var currentUserId = CurrentUserId();
            var eventItem = _context.Etkinlikler
                .Include(item => item.Participants)
                .Include(item => item.StaffMembers)
                .FirstOrDefault(item => item.Id == eventId);
            if (eventItem is null) return NotFound();
            if (!CanAssignStaff(eventItem, currentUserId)) return Forbid();
            if (role is not (EventStaffRole.Assistant or EventStaffRole.AttendanceOfficer))
            {
                TempData["ErrorMessage"] = "Geçersiz görev rolü.";
                return RedirectToAction("Community", new { id = eventId });
            }

            var canBeAssigned = eventItem.Participants.Any(item =>
                item.KullaniciId == userId &&
                item.Status is ParticipationStatus.Registered or ParticipationStatus.Attended);
            if (!canBeAssigned || userId == eventItem.CreatedByUserId)
            {
                TempData["ErrorMessage"] = "Sadece etkinliğe katılan kullanıcılar görevli yapılabilir.";
                return RedirectToAction("Community", new { id = eventId });
            }

            if (!_context.EventStaff.Any(item => item.EtkinlikId == eventId && item.UserId == userId && item.Role == role))
            {
                _context.EventStaff.Add(new EventStaff
                {
                    EtkinlikId = eventId,
                    UserId = userId,
                    Role = role,
                    CreatedByUserId = currentUserId,
                    CreatedAtUtc = DateTime.UtcNow
                });
                Audit("EventStaffAssigned", nameof(EventStaff), $"{eventId}:{userId}:{role}");
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Görevli atandı.";
            }

            return RedirectToAction("Community", new { id = eventId });
        }

        [HttpPost]
        public IActionResult RemoveEventStaff(int staffId)
        {
            var staff = _context.EventStaff
                .Include(item => item.Etkinlik)
                .ThenInclude(item => item.Participants)
                .FirstOrDefault(item => item.Id == staffId);
            if (staff is null) return NotFound();
            if (!CanAssignStaff(staff.Etkinlik, CurrentUserId())) return Forbid();

            var eventId = staff.EtkinlikId;
            _context.EventStaff.Remove(staff);
            Audit("EventStaffRemoved", nameof(EventStaff), staff.Id);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Görev kaldırıldı.";
            return RedirectToAction("Community", new { id = eventId });
        }

        [HttpGet]
        public IActionResult GenerateAttendanceQr(int eventId)
        {
            var userId = CurrentUserId();
            var eventItem = _context.Etkinlikler
                .Include(item => item.Participants)
                .FirstOrDefault(item => item.Id == eventId && item.Status == EventStatus.Approved);
            if (eventItem is null) return NotFound();
            if (!IsAttendanceWindowOpen(eventItem))
                return BadRequest(new { message = "QR yoklama sadece etkinlik zamanına yakın açılır." });

            var participation = eventItem.Participants.FirstOrDefault(item =>
                item.KullaniciId == userId && item.Status == ParticipationStatus.Registered);
            if (participation is null)
                return Forbid();

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(5);

            _context.AttendanceQrTokens.Add(new AttendanceQrToken
            {
                EtkinlikId = eventId,
                ParticipantId = participation.Id,
                TokenHash = HashToken(token),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = expiresAtUtc
            });
            _context.SaveChanges();

            return Json(new { token, expiresAtUtc });
        }

        [HttpPost]
        public IActionResult VerifyAttendanceQr(int eventId, string token)
        {
            var currentUserId = CurrentUserId();
            var eventItem = _context.Etkinlikler
                .Include(item => item.Participants)
                .Include(item => item.StaffMembers)
                .FirstOrDefault(item => item.Id == eventId && item.Status == EventStatus.Approved);
            if (eventItem is null) return NotFound();
            if (!CanTakeAttendance(eventItem, currentUserId)) return Forbid();
            if (!IsAttendanceWindowOpen(eventItem))
                return BadRequest(new { message = "Yoklama zamanı açık değil." });
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "QR kod boş." });

            var tokenHash = HashToken(token.Trim());
            var qrToken = _context.AttendanceQrTokens
                .Include(item => item.Participant)
                .ThenInclude(item => item.Kullanici)
                .FirstOrDefault(item => item.EtkinlikId == eventId && item.TokenHash == tokenHash);

            if (qrToken is null || qrToken.ExpiresAtUtc < DateTime.UtcNow)
                return BadRequest(new { message = "QR kod geçersiz veya süresi dolmuş." });
            if (qrToken.UsedAtUtc is not null)
                return Conflict(new { message = "Bu QR kod daha önce kullanılmış." });
            if (qrToken.Participant.Status != ParticipationStatus.Registered)
                return Conflict(new { message = "Katılımcı yoklamaya uygun durumda değil." });

            qrToken.UsedAtUtc = DateTime.UtcNow;
            qrToken.UsedByUserId = currentUserId;
            qrToken.Participant.Status = ParticipationStatus.Attended;
            qrToken.Participant.AttendanceConfirmedAtUtc = DateTime.UtcNow;
            qrToken.Participant.AttendanceConfirmedByUserId = currentUserId;
            AwardAttendanceScores(qrToken.Participant);
            Audit("QrAttendanceConfirmed", nameof(Participant), qrToken.Participant.Id);
            _context.SaveChanges();

            return Json(new
            {
                message = $"{qrToken.Participant.Kullanici.UserName} için yoklama alındı.",
                participantName = qrToken.Participant.Kullanici.UserName
            });
        }





        //*--------------------------------------------Etkinlik Katılma-----------------------------------------------------------------------------------------------------------------*//
        [HttpPost]
        public async Task<IActionResult> JoinEvent(int EtkinlikId)
        {
            // Kullanıcının oturum bilgileri
            var userIdInt = CurrentUserId();
            var currentUser = _context.Users.FirstOrDefault(item => item.Id == userIdInt);
            if (currentUser is null || !IsProfileComplete(currentUser))
            {
                TempData["ErrorMessage"] = "Etkinliğe katılmadan önce profil bilgilerini tamamlamalısın.";
                return RedirectToAction("Profile", "User");
            }

            // Katılmak istenilen etkinlik
            var newEvent = _context.Etkinlikler.FirstOrDefault(e => e.Id == EtkinlikId);
            if (newEvent == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("Index", "User");
            }

            if (newEvent.Status != EventStatus.Approved || newEvent.Date <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Yalnızca onaylanmış ve başlamamış etkinliklere katılabilirsiniz.";
                return RedirectToAction("Details", new { id = EtkinlikId });
            }

            if (newEvent.CreatedByUserId == userIdInt)
            {
                TempData["ErrorMessage"] = "Kendi etkinliğinize katılımcı olarak kaydolamazsınız.";
                return RedirectToAction("Details", new { id = EtkinlikId });
            }

            var registeredCount = _context.Katilimlar.Count(item =>
                item.EtkinlikId == EtkinlikId && item.Status == ParticipationStatus.Registered);
            if (newEvent.MaxParticipants is > 0 && registeredCount >= newEvent.MaxParticipants)
            {
                TempData["ErrorMessage"] = "Bu etkinliğin katılım kontenjanı doldu.";
                return RedirectToAction("Details", new { id = EtkinlikId });
            }

            var existingParticipation = _context.Katilimlar
                .FirstOrDefault(item => item.KullaniciId == userIdInt && item.EtkinlikId == EtkinlikId);
            if (existingParticipation is not null)
            {
                if (existingParticipation.Status == ParticipationStatus.Cancelled)
                {
                    existingParticipation.Status = ParticipationStatus.Registered;
                    existingParticipation.ParticipationDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Etkinliğe yeniden katıldınız.";
                    return RedirectToAction("Community", new { id = EtkinlikId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Bu etkinliğe zaten katıldınız.";
                }
                return RedirectToAction("Details", new { id = EtkinlikId });
            }

            // Kullanıcının daha önce katıldığı etkinlikler
            var userEvents = _context.Katilimlar
                                     .Where(p => p.KullaniciId == userIdInt && p.Status == ParticipationStatus.Registered)
                                     .Select(p => p.Etkinlik)
                                     .ToList();

            // Yeni etkinliğin başlangıç ve bitiş zaman ayarı
            var newEventStart = newEvent.Date;
            var newEventEnd = newEvent.Date.Add(newEvent.Duration);

            // Zaman çakışması kontrolü
            foreach (var userEvent in userEvents)
            {
                var userEventStart = userEvent.Date;
                var userEventEnd = userEvent.Date.Add(userEvent.Duration);

                if (newEventStart < userEventEnd && newEventEnd > userEventStart)
                {
                    TempData["ErrorMessage"] = $"Etkinlik çakışması: \"{userEvent.Name}\" adlı etkinlikle aynı zaman diliminde.";
                    return RedirectToAction("Details", new { id = EtkinlikId });
                }
            }

            // Çakışma yoksa etkinliğe katıl
            _context.Katilimlar.Add(new Participant
            {
                KullaniciId = userIdInt,
                EtkinlikId = EtkinlikId,
                ParticipationDate = DateTime.UtcNow,
                Status = ParticipationStatus.Registered
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // The unique database constraint is the final guard for simultaneous requests.
                TempData["ErrorMessage"] = "Bu etkinliğe katılımınız zaten kaydedilmiş olabilir. Sayfayı yenileyin.";
                return RedirectToAction("Details", new { id = EtkinlikId });
            }

            TempData["SuccessMessage"] = "Etkinliğe başarıyla katıldınız.";
            return RedirectToAction("Community", new { id = EtkinlikId });
        }




        //*------------------------------------------- Etkinlik Güncellme İşlemi-------------------------------------------------------------------------------------------------*//

        [HttpGet]
        public IActionResult UpdateEvent(int id)
        {
            var etkinlik = _context.Etkinlikler.Include(e => e.EventCategories)
                                               .FirstOrDefault(e => e.Id == id &&
                                                   (e.CreatedByUserId == CurrentUserId() || User.IsInRole("Admin")));

            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("MyEvent", "User");
            }

            // Kategorileri gönder
            ViewBag.Categories = _context.Kategoriler.Where(item => !item.IsArchived).ToList();

            return View(etkinlik);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateEvent(int id, Etkinlik model, IFormFile Image, int selectedCategoryId, int DurationHours, int DurationMinutes)
        {
            var etkinlik = _context.Etkinlikler.Include(e => e.EventCategories)
                                               .FirstOrDefault(e => e.Id == id &&
                                                   (e.CreatedByUserId == CurrentUserId() || User.IsInRole("Admin")));

            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("MyEvent", "User");
            }

            if (model.RowVersion is null || model.RowVersion.Length == 0)
            {
                TempData["ErrorMessage"] = "Etkinlik sürüm bilgisi eksik. Sayfayı yenileyip tekrar deneyin.";
                return RedirectToAction("UpdateEvent", new { id });
            }

            _context.Entry(etkinlik).Property(item => item.RowVersion).OriginalValue = model.RowVersion;

            if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length > 100 ||
                model.Description?.Length > 500 || model.Date <= DateTime.Now ||
                model.DetailedProgram?.Length > 3000 ||
                DurationHours is < 0 or > 23 || DurationMinutes is < 0 or > 59 ||
                (DurationHours == 0 && DurationMinutes == 0) ||
                model.MaxParticipants is < 0 or > 100000 ||
                string.IsNullOrWhiteSpace(model.Location) || model.Location.Trim().Length > 250 ||
                model.Latitude is < -90 or > 90 || model.Longitude is < -180 or > 180 ||
                !_context.Kategoriler.Any(item => item.Id == selectedCategoryId && !item.IsArchived))
            {
                TempData["ErrorMessage"] = "Etkinlik bilgileri geçersiz.";
                return RedirectToAction("UpdateEvent", new { id });
            }

            // Etkinlik bilgilerini güncelle
            etkinlik.Name = model.Name.Trim();
            etkinlik.Description = model.Description?.Trim();
            etkinlik.DetailedProgram = model.DetailedProgram?.Trim();
            etkinlik.Date = model.Date;
            etkinlik.Duration = new TimeSpan(DurationHours, DurationMinutes, 0);
            etkinlik.Location = model.Location.Trim();
            etkinlik.MaxParticipants = model.MaxParticipants is > 0 ? model.MaxParticipants : null;
            etkinlik.Latitude = model.Latitude;
            etkinlik.Longitude = model.Longitude;
            if (etkinlik.Status == EventStatus.Approved)
            {
                var approvalScore = _context.Puanlar.FirstOrDefault(item =>
                    item.KullaniciId == etkinlik.CreatedByUserId && item.Reason == ScoreReason.EventApproved &&
                    item.SourceId == etkinlik.Id && item.RevokedAtUtc == null);
                if (approvalScore is not null) approvalScore.RevokedAtUtc = DateTime.UtcNow;
            }
            etkinlik.Status = EventStatus.Pending;
            etkinlik.UpdatedAtUtc = DateTime.UtcNow;

            // Yeni resim eklenmişse kaydet
            if (Image != null && Image.Length > 0)
            {
                try
                {
                    etkinlik.ImageUrl = await _imageStorage.StoreAsync(Image, 5 * 1024 * 1024);
                }
                catch (InvalidDataException exception)
                {
                    TempData["ErrorMessage"] = exception.Message;
                    return RedirectToAction("UpdateEvent", new { id });
                }
            }

            // Kategori güncellemesi
            etkinlik.EventCategories.Clear();
            etkinlik.EventCategories.Add(new EventCategory { KategoriId = selectedCategoryId, EtkinlikId = id });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Etkinlik siz düzenlerken değiştirildi. Güncel bilgileri kontrol edip tekrar deneyin.";
                return RedirectToAction("UpdateEvent", new { id });
            }

            TempData["SuccessMessage"] = "Etkinlik başarıyla güncellendi!";
            return RedirectToAction("MyEvent", "User");
        }

        [HttpPost]
        public IActionResult LeaveEvent(int etkinlikId)
        {
            var participation = _context.Katilimlar.FirstOrDefault(item =>
                item.KullaniciId == CurrentUserId() && item.EtkinlikId == etkinlikId);
            if (participation is null) return NotFound();
            if (participation.Status != ParticipationStatus.Registered)
                return Conflict("Katılım artık iptal edilemez.");

            participation.Status = ParticipationStatus.Cancelled;
            Audit("ParticipationCancelled", nameof(Participant), participation.Id);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Etkinlik katılımınız iptal edildi.";
            return RedirectToAction("Details", new { id = etkinlikId });
        }

        [HttpPost]
        public IActionResult CancelEvent(int id)
        {
            var eventItem = _context.Etkinlikler.FirstOrDefault(item => item.Id == id &&
                (item.CreatedByUserId == CurrentUserId() || User.IsInRole("Admin")));
            if (eventItem is null) return NotFound();
            if (eventItem.Status is EventStatus.Completed or EventStatus.Cancelled)
                return Conflict("Etkinlik bu durumda iptal edilemez.");

            eventItem.Status = EventStatus.Cancelled;
            eventItem.UpdatedAtUtc = DateTime.UtcNow;
            var approvalScore = _context.Puanlar.FirstOrDefault(item =>
                item.KullaniciId == eventItem.CreatedByUserId && item.Reason == ScoreReason.EventApproved &&
                item.SourceId == eventItem.Id && item.RevokedAtUtc == null);
            if (approvalScore is not null) approvalScore.RevokedAtUtc = DateTime.UtcNow;
            Audit("EventCancelled", nameof(Etkinlik), eventItem.Id);
            _context.SaveChanges();
            return RedirectToAction("MyEvent", "User");
        }

        [HttpPost]
        public IActionResult ConfirmAttendance(int eventId, int[] attendedUserIds)
        {
            var eventItem = _context.Etkinlikler.Include(item => item.Participants)
                .FirstOrDefault(item => item.Id == eventId &&
                    (item.CreatedByUserId == CurrentUserId() || User.IsInRole("Admin")));
            if (eventItem is null) return NotFound();
            if (eventItem.Date.Add(eventItem.Duration) > DateTime.Now)
                return Conflict("Yoklama etkinlik bitmeden tamamlanamaz.");
            if (eventItem.Status != EventStatus.Approved)
                return Conflict("Yoklama yalnızca onaylı bir etkinlik için tamamlanabilir.");

            var attended = attendedUserIds.ToHashSet();
            foreach (var participation in eventItem.Participants.Where(item => item.Status == ParticipationStatus.Registered))
            {
                participation.Status = attended.Contains(participation.KullaniciId)
                    ? ParticipationStatus.Attended
                    : ParticipationStatus.NoShow;
                participation.AttendanceConfirmedAtUtc = DateTime.UtcNow;
                participation.AttendanceConfirmedByUserId = CurrentUserId();

                if (participation.Status == ParticipationStatus.Attended)
                    AwardAttendanceScores(participation);
            }

            eventItem.Status = EventStatus.Completed;
            eventItem.UpdatedAtUtc = DateTime.UtcNow;
            Audit("AttendanceConfirmed", nameof(Etkinlik), eventItem.Id);
            _context.SaveChanges();
            return RedirectToAction("MyEvent", "User");
        }

        private void AwardAttendanceScores(Participant participation)
        {
            if (!_context.Puanlar.Any(item => item.KullaniciId == participation.KullaniciId &&
                                              item.Reason == ScoreReason.Attendance &&
                                              item.SourceId == participation.EtkinlikId))
            {
                _context.Puanlar.Add(new Score
                {
                    KullaniciId = participation.KullaniciId,
                    Value = 10,
                    Reason = ScoreReason.Attendance,
                    SourceId = participation.EtkinlikId,
                    EarnedDate = DateTime.UtcNow
                });
            }

            var hasPriorAttendance = _context.Puanlar.Any(item => item.KullaniciId == participation.KullaniciId &&
                item.Reason == ScoreReason.FirstAttendanceBonus);
            if (!hasPriorAttendance)
            {
                _context.Puanlar.Add(new Score
                {
                    KullaniciId = participation.KullaniciId,
                    Value = 20,
                    Reason = ScoreReason.FirstAttendanceBonus,
                    SourceId = participation.EtkinlikId,
                    EarnedDate = DateTime.UtcNow
                });
            }
        }

        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Authenticated user id is missing."));

        private bool IsProfileComplete(Kullanici user) =>
            !string.IsNullOrWhiteSpace(user.FirstName) &&
            !string.IsNullOrWhiteSpace(user.LastName) &&
            user.BirthDate != default &&
            !string.IsNullOrWhiteSpace(user.Gender) &&
            !string.IsNullOrWhiteSpace(user.Location) &&
            user.Latitude.HasValue &&
            user.Longitude.HasValue;

        private bool CanAccessCommunity(Etkinlik etkinlik, int userId) =>
            etkinlik.CreatedByUserId == userId ||
            etkinlik.Participants.Any(item =>
                item.KullaniciId == userId &&
                item.Status is ParticipationStatus.Registered or ParticipationStatus.Attended) ||
            _context.EventStaff.Any(item => item.EtkinlikId == etkinlik.Id && item.UserId == userId);

        private bool CanManageCommunity(Etkinlik etkinlik, int userId) =>
            etkinlik.CreatedByUserId == userId ||
            User.IsInRole("Admin") ||
            _context.EventStaff.Any(item =>
                item.EtkinlikId == etkinlik.Id &&
                item.UserId == userId &&
                item.Role == EventStaffRole.Assistant);

        private bool CanAssignStaff(Etkinlik etkinlik, int userId) =>
            etkinlik.CreatedByUserId == userId || User.IsInRole("Admin");

        private bool CanTakeAttendance(Etkinlik etkinlik, int userId) =>
            etkinlik.CreatedByUserId == userId ||
            User.IsInRole("Admin") ||
            _context.EventStaff.Any(item =>
                item.EtkinlikId == etkinlik.Id &&
                item.UserId == userId &&
                item.Role == EventStaffRole.AttendanceOfficer);

        private static bool IsAttendanceWindowOpen(Etkinlik etkinlik)
        {
            var now = DateTime.Now;
            return now >= etkinlik.Date.AddMinutes(-30) &&
                   now <= etkinlik.Date.Add(etkinlik.Duration).AddHours(2);
        }

        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private void Audit(string action, string entityType, object entityId) =>
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = CurrentUserId(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId.ToString() ?? string.Empty,
                CorrelationId = HttpContext.TraceIdentifier
            });



    }
}
