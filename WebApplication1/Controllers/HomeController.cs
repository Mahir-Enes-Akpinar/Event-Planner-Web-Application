using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly SignInManager<Kullanici> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;

    public HomeController(
        UserManager<Kullanici> userManager,
        SignInManager<Kullanici> signInManager,
        IEmailService emailService,
        ILogger<HomeController> logger,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger;
        _environment = environment;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return View();

        if (User.IsInRole("Admin"))
        {
            var user = await _userManager.GetUserAsync(User);
            return user is not null && !user.TwoFactorEnabled
                ? RedirectToAction(nameof(SetupTwoFactor))
                : RedirectToAction("Index", "Admin");
        }
        return RedirectToAction("Index", "User");
    }

    [AllowAnonymous, HttpPost, EnableRateLimiting("authentication")]
    public async Task<IActionResult> Login(string userName, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(
            userName, password, isPersistent: false, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(TwoFactorLogin));

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.IsLockedOut
                ? "Çok fazla başarısız deneme yapıldı. Lütfen daha sonra tekrar deneyin."
                : result.IsNotAllowed
                    ? "E-posta adresiniz henüz doğrulanmamış. Kayıt sırasında gönderilen doğrulama bağlantısını açın."
                    : "Kullanıcı adı veya şifre yanlış.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user is null || user.IsDeleted)
        {
            await _signInManager.SignOutAsync();
            TempData["ErrorMessage"] = "Hesap kullanılamıyor.";
            return RedirectToAction(nameof(Index));
        }

        SetCompatibilitySession(user);
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            if (!await _userManager.GetTwoFactorEnabledAsync(user))
                return RedirectToAction(nameof(SetupTwoFactor));
            return RedirectToAction("Index", "Admin");
        }

        return RedirectToAction("Index", "User");
    }

    [AllowAnonymous, HttpPost, EnableRateLimiting("authentication")]
    public async Task<IActionResult> Register(string userName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Trim().Length > 50 ||
            string.IsNullOrWhiteSpace(email) || email.Trim().Length > 256 ||
            string.IsNullOrWhiteSpace(password) || password.Length > 128)
        {
            TempData["ErrorMessage"] = "Kayıt bilgileri eksik veya geçersiz.";
            return RedirectToAction(nameof(Index));
        }

        var user = new Kullanici
        {
            UserName = userName.Trim(),
            Email = email.Trim(),
            EmailConfirmed = _environment.IsDevelopment(),
            FirstName = "Belirtilmedi",
            LastName = "Belirtilmedi",
            BirthDate = new DateTime(1900, 1, 1),
            Gender = "Belirtilmedi",
            ProfilePicture = "/images/default-profile.png"
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        await _userManager.AddToRoleAsync(user, "User");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl = Url.Action(nameof(ConfirmEmail), "Home",
            new { userId = user.Id, token }, Request.Scheme)!;

        await _emailService.SendAsync(user.Email!, "E-posta adresinizi doğrulayın",
            $"Hesabınızı doğrulamak için bağlantıyı açın: {confirmationUrl}");

        var registrationMessage = "Kayıt tamamlandı. E-posta doğrulama bağlantısı gönderildi.";
        if (_environment.IsDevelopment())
            registrationMessage += $" Yerel geliştirme bağlantısı: {confirmationUrl}";
        TempData["SuccessMessage"] = registrationMessage;
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> ConfirmEmail(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
            ? "E-posta adresiniz doğrulandı."
            : "Doğrulama bağlantısı geçersiz veya süresi dolmuş.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult TwoFactorLogin() => View();

    [AllowAnonymous, HttpPost, EnableRateLimiting("authentication")]
    public async Task<IActionResult> TwoFactorLogin(string code)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null) return RedirectToAction(nameof(Index));
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            code.Replace(" ", string.Empty).Replace("-", string.Empty), false, false);
        if (!result.Succeeded)
        {
            ViewBag.Error = "Doğrulama kodu geçersiz.";
            return View();
        }

        SetCompatibilitySession(user);
        return RedirectToAction("Index", "Admin");
    }

    [Authorize(Roles = "Admin"), HttpGet]
    public async Task<IActionResult> SetupTwoFactor()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }
        ViewData["SharedKey"] = key;
        return View();
    }

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> SetupTwoFactor(string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        var valid = await _userManager.VerifyTwoFactorTokenAsync(user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code.Replace(" ", string.Empty).Replace("-", string.Empty));
        if (!valid)
        {
            ViewBag.Error = "Doğrulama kodu geçersiz.";
            ViewData["SharedKey"] = await _userManager.GetAuthenticatorKeyAsync(user);
            return View();
        }
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        await _signInManager.RefreshSignInAsync(user);
        return RedirectToAction("Index", "Admin");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult ForgotPassword() => View();

    [AllowAnonymous, HttpPost, EnableRateLimiting("authentication")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url.Action(nameof(VerifyCode), "Home",
                new { email = user.Email, token }, Request.Scheme)!;
            await _emailService.SendAsync(user.Email!, "Şifre sıfırlama",
                $"Şifrenizi sıfırlamak için bağlantıyı açın: {resetUrl}");
        }

        TempData["SuccessMessage"] = "Adres kayıtlıysa şifre sıfırlama bağlantısı gönderildi.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult VerifyCode(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(ForgotPassword));

        ViewData["Email"] = email;
        ViewData["Token"] = token;
        return View();
    }

    [AllowAnonymous, HttpPost, EnableRateLimiting("authentication")]
    public async Task<IActionResult> VerifyCode(string email, string verificationCode, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            TempData["ErrorMessage"] = "Sıfırlama bağlantısı geçersiz veya süresi dolmuş.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var result = await _userManager.ResetPasswordAsync(user, verificationCode, newPassword);
        if (!result.Succeeded)
        {
            ViewData["Email"] = email;
            ViewData["Token"] = verificationCode;
            ViewBag.Error = string.Join(" ", result.Errors.Select(error => error.Description));
            return View();
        }

        TempData["SuccessMessage"] = "Şifreniz güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public IActionResult Dashboard() => RedirectToAction("Index", "User");

    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard() => RedirectToAction("Index", "Admin");

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [AllowAnonymous]
    public IActionResult AccessDenied() => View("AccessDenied");

    [AllowAnonymous]
    public IActionResult HttpError(int code)
    {
        Response.StatusCode = code;
        ViewData["StatusCode"] = code;
        return View("HttpError");
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogError("Unhandled request failure. RequestId: {RequestId}", requestId);
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = requestId });
    }

    private void SetCompatibilitySession(Kullanici user)
    {
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.UserName ?? string.Empty);
        HttpContext.Session.SetString("UserProfilePicture", user.ProfilePicture ?? "/images/default-profile.png");
    }
}
