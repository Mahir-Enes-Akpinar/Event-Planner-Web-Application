using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApplication1.Services;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly Context _context;
        private readonly IImageStorage _imageStorage;
        private readonly SignInManager<Kullanici> _signInManager;
        private readonly IGeoService _geoService;

        public UserController(Context context, IImageStorage imageStorage, SignInManager<Kullanici> signInManager, IGeoService geoService)
        {
            _context = context;
            _imageStorage = imageStorage;
            _signInManager = signInManager;
            _geoService = geoService;
        }

        public IActionResult Index(string? query, int? categoryId, DateTime? from, DateTime? to, int page = 1, int pageSize = 12)
        {
            var userId = CurrentUserId();
            var userName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
            ViewData["UserName"] = userName;
            ViewData["UserProfilePicture"] = HttpContext.Session.GetString("UserProfilePicture") ?? "~/images/profile.png";

            // İLGİ ALANI VE GEÇMİŞ KATILDIĞI ETKİNLİKLER 
            var userInterests = _context.KullaniciKategorileri
                                        .Where(kc => kc.KullaniciId == userId)
                                        .Select(kc => kc.KategoriId)
                                        .ToList();

            var pastEventCategories = _context.Katilimlar
                                              .Where(k => k.KullaniciId == userId)
                                              .SelectMany(k => k.Etkinlik.EventCategories)
                                              .Select(ec => ec.KategoriId)
                                              .Distinct()
                                              .ToList();

            var combinedCategories = userInterests.Concat(pastEventCategories).Distinct().ToList();

            var interestAndPastBasedEvents = _context.EtkinlikKategorileri
                                                     .Where(ek => combinedCategories.Contains(ek.KategoriId) &&
                                                                  ek.Etkinlik.Status == EventStatus.Approved &&
                                                                  ek.Etkinlik.Date >= DateTime.Now)
                                                     .Select(ek => ek.Etkinlik)
                                                     .Distinct()
                                                     .Include(e => e.Participants)
                                                     .ToList();

            // Konum bazlı etkinlikler
            var userLocation = _context.Kullanicilar
                               .Where(u => u.Id == userId)
                               .Select(u => new { u.Latitude, u.Longitude })
                               .FirstOrDefault();

            var locationBasedEvents = new List<Etkinlik>();
            if (userLocation?.Latitude is not null && userLocation.Longitude is not null)
            {
                locationBasedEvents = _context.Etkinlikler
                    .Where(e => e.Status == EventStatus.Approved && e.Date >= DateTime.Now &&
                                e.Latitude != 0 && e.Longitude != 0)
                    .Include(e => e.Participants)
                    .AsEnumerable()
                    .Where(e => DistanceKm(userLocation.Latitude.Value, userLocation.Longitude.Value,
                                           e.Latitude, e.Longitude) <= 50)
                    .ToList();
            }

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var catalog = _context.Etkinlikler
                .Where(e => e.Status == EventStatus.Approved && e.Date >= DateTime.Now);
            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim();
                catalog = catalog.Where(item => item.Name.Contains(term) ||
                    (item.Description != null && item.Description.Contains(term)) || item.Location.Contains(term));
            }
            if (categoryId.HasValue)
                catalog = catalog.Where(item => item.EventCategories.Any(link => link.KategoriId == categoryId));
            if (from.HasValue) catalog = catalog.Where(item => item.Date >= from.Value);
            if (to.HasValue) catalog = catalog.Where(item => item.Date <= to.Value);

            var totalCount = catalog.Count();
            var allEvents = catalog.OrderBy(item => item.Date)
                .Include(item => item.Participants)
                .Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Categories = _context.Kategoriler.Where(item => !item.IsArchived).OrderBy(item => item.Name).ToList();
            ViewBag.Query = query;
            ViewBag.CategoryId = categoryId;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Page = page;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

            var viewModel = Tuple.Create(interestAndPastBasedEvents, locationBasedEvents, allEvents);

            return View(viewModel);
        }



        private static double DistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            const double earthRadiusKm = 6371;
            static double Radians(double value) => value * Math.PI / 180;
            var deltaLat = Radians(latitude2 - latitude1);
            var deltaLon = Radians(longitude2 - longitude1);
            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(Radians(latitude1)) * Math.Cos(Radians(latitude2)) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }






















        // İlgi Alanı Ekleme
        public IActionResult SelectInterests()
        {
            var categories = _context.Kategoriler.Where(item => !item.IsArchived).ToList();
            return View(categories);
        }

        [HttpPost]
        public IActionResult SaveInterests(int[] selectedCategories)
        {
            var userId = CurrentUserId();

            var validCategoryIds = _context.Kategoriler
                .Where(category => !category.IsArchived && selectedCategories.Contains(category.Id))
                .Select(category => category.Id)
                .ToHashSet();

            var existingInterests = _context.KullaniciKategorileri.Where(kc => kc.KullaniciId == userId);
            _context.KullaniciKategorileri.RemoveRange(existingInterests);

            foreach (var categoryId in validCategoryIds)
            {
                _context.KullaniciKategorileri.Add(new UserInterest
                {
                    KullaniciId = userId,
                    KategoriId = categoryId
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "İlgi alanlarınız başarıyla kaydedildi.";
            return RedirectToAction("Index", "User");
        }

        // Etkinliklerim Kısmı
        public IActionResult MyEvent()
        {

            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Misafir Kullanıcı";
            ViewData["UserProfilePicture"] = HttpContext.Session.GetString("UserProfilePicture") ?? "~/images/profile.png";


            var userId = CurrentUserId();

            // Kullanıcının oluşturduğu etkinlikler
            var createdEvents = _context.Etkinlikler
                                        .Where(e => e.CreatedByUserId == userId)
                                        .Include(e => e.Participants)
                                        .ToList();

            // Kullanıcının katıldığı etkinlikler
            var participatedEvents = _context.Katilimlar
                                             .Include(k => k.Etkinlik) // Etkinlik verilerini dahil et
                                             .ThenInclude(e => e.Participants) // Etkinlik katılımcılarını dahil et
                                             .Where(p => p.KullaniciId == userId &&
                                                 (p.Status == ParticipationStatus.Registered || p.Status == ParticipationStatus.Attended))
                                             .Select(p => p.Etkinlik)
                                             .ToList();


            return View(Tuple.Create<IEnumerable<Etkinlik>, IEnumerable<Etkinlik>>(createdEvents, participatedEvents));
        }


        //*---------------------------------------------------------- PUAN TABLOSU--------------------------------------------------------------------------------------*//
        public IActionResult Leaderboard()
        {
            var leaderboard = _context.Puanlar.Where(score => score.RevokedAtUtc == null)
                                      .GroupBy(s => s.KullaniciId)
                                      .Select(g => new
                                      {
                                          KullaniciId = g.Key,
                                          TotalScore = g.Sum(s => s.Value),
                                          Kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == g.Key)
                                      })
                                      .OrderByDescending(x => x.TotalScore)
                                      .ToList();

            return View(leaderboard);
        }


        //Profil Kısmı
        public IActionResult Profile()
        {
            var userId = CurrentUserId();

            var user = _context.Kullanicilar.Include(u => u.Scores).FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Kullanıcının toplam puanını hesapla
            var totalScore = _context.Puanlar
                .Where(s => s.KullaniciId == userId && s.RevokedAtUtc == null)
                .Sum(s => s.Value);

            ViewData["TotalScore"] = totalScore;
            return View(user);
        }


        //İLGİLERİ GÜNCELLEME
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Kullanici updatedUser, IFormFile profileImage, double? latitude, double? longitude)
        {
            var userId = CurrentUserId();

            var user = _context.Kullanicilar.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Profile");
            }

            if (string.IsNullOrWhiteSpace(updatedUser.FirstName) || updatedUser.FirstName.Trim().Length > 50 ||
                string.IsNullOrWhiteSpace(updatedUser.LastName) || updatedUser.LastName.Trim().Length > 50 ||
                updatedUser.BirthDate == default || updatedUser.BirthDate > DateTime.Today ||
                string.IsNullOrWhiteSpace(updatedUser.Gender) || updatedUser.Gender.Trim().Length > 20 ||
                updatedUser.PhoneNumber?.Length > 15 || updatedUser.Location?.Length > 250 ||
                latitude is < -90 or > 90 || longitude is < -180 or > 180)
            {
                TempData["ErrorMessage"] = "Profil bilgileri geçersiz.";
                return RedirectToAction("Profile");
            }

            // Bilgileri güncelle
            user.FirstName = updatedUser.FirstName.Trim();
            user.LastName = updatedUser.LastName.Trim();
            user.BirthDate = updatedUser.BirthDate;
            user.Gender = updatedUser.Gender.Trim();
            user.PhoneNumber = updatedUser.PhoneNumber?.Trim();
            user.Location = updatedUser.Location?.Trim();
            user.UpdatedAtUtc = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(updatedUser.Location) && latitude.HasValue && longitude.HasValue && (latitude.Value != 0 || longitude.Value != 0))
            {
                user.Latitude = latitude.Value;
                user.Longitude = longitude.Value;
            }
            else if (!string.IsNullOrWhiteSpace(updatedUser.Location))
            {
                try
                {
                    var match = (await _geoService.SearchAsync(updatedUser.Location)).FirstOrDefault();
                    if (match is not null)
                    {
                        user.Location = match.DisplayName;
                        user.Latitude = match.Latitude;
                        user.Longitude = match.Longitude;
                    }
                }
                catch (HttpRequestException)
                {
                    // Manual address remains valid when the external provider is unavailable.
                }
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                try
                {
                    user.ProfilePicture = await _imageStorage.StoreAsync(profileImage, 2 * 1024 * 1024);
                }
                catch (InvalidDataException ex)
                {
                    TempData["ErrorMessage"] = $"Resim yüklenirken hata oluştu: {ex.Message}";
                }
            }


            _context.SaveChanges();
            TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = _context.Kullanicilar.FirstOrDefault(item => item.Id == CurrentUserId());
            if (user is null) return NotFound();

            user.IsDeleted = true;
            user.DeletedAtUtc = DateTime.UtcNow;
            user.AnonymizeAtUtc = DateTime.UtcNow.AddDays(30);
            user.SecurityStamp = Guid.NewGuid().ToString();
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = user.Id,
                Action = "AccountDeletionScheduled",
                EntityType = nameof(Kullanici),
                EntityId = user.Id.ToString(),
                CorrelationId = HttpContext.TraceIdentifier
            });
            await _context.SaveChangesAsync();
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Authenticated user id is missing."));
    }
}
