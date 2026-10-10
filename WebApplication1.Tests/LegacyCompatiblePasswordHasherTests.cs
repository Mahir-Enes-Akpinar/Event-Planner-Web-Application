using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests;

public sealed class LegacyCompatiblePasswordHasherTests
{
    private readonly LegacyCompatiblePasswordHasher _hasher = new();
    private readonly Kullanici _user = new() { UserName = "test", FirstName = "Test", LastName = "User", Gender = "-" };

    [Fact]
    public void LegacySha256Password_IsAcceptedAndMarkedForRehash()
    {
        const string password = "Secure!Password1";
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();

        var result = _hasher.VerifyHashedPassword(_user, legacyHash, password);

        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, result);
    }

    [Fact]
    public void WrongLegacyPassword_IsRejected()
    {
        var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Correct!Password1"))).ToLowerInvariant();

        var result = _hasher.VerifyHashedPassword(_user, legacyHash, "Wrong!Password1");

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void IdentityPassword_RoundTrips()
    {
        const string password = "Secure!Password1";
        var hash = _hasher.HashPassword(_user, password);

        var result = _hasher.VerifyHashedPassword(_user, hash, password);

        Assert.NotEqual(PasswordVerificationResult.Failed, result);
    }
}
