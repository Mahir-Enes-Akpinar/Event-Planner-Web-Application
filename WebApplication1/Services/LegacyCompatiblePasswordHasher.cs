using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class LegacyCompatiblePasswordHasher : IPasswordHasher<Kullanici>
{
    private readonly PasswordHasher<Kullanici> _identityHasher = new();

    public string HashPassword(Kullanici user, string password) =>
        _identityHasher.HashPassword(user, password);

    public PasswordVerificationResult VerifyHashedPassword(
        Kullanici user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.Length == 64 && hashedPassword.All(Uri.IsHexDigit))
        {
            var computed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(providedPassword)))
                .ToLowerInvariant();
            var matches = CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(hashedPassword.ToLowerInvariant()),
                Encoding.ASCII.GetBytes(computed));
            return matches
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }

        return _identityHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }
}
