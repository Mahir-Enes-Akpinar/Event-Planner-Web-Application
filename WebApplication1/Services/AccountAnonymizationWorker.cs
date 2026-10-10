using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class AccountAnonymizationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountAnonymizationWorker> _logger;

    public AccountAnonymizationWorker(IServiceScopeFactory scopeFactory, ILogger<AccountAnonymizationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await AnonymizeDueAccounts(stoppingToken); }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "Account anonymization cycle failed.");
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task AnonymizeDueAccounts(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<Context>();
        var dueUsers = await database.Kullanicilar.IgnoreQueryFilters()
            .Where(user => user.IsDeleted && user.AnonymizeAtUtc <= DateTime.UtcNow && user.Email != null)
            .ToListAsync(cancellationToken);

        foreach (var user in dueUsers)
        {
            var anonymousName = $"deleted-{user.Id}";
            user.UserName = anonymousName;
            user.NormalizedUserName = anonymousName.ToUpperInvariant();
            user.Email = $"{anonymousName}@invalid.local";
            user.NormalizedEmail = user.Email.ToUpperInvariant();
            user.FirstName = "Silinmiş";
            user.LastName = "Kullanıcı";
            user.PhoneNumber = null;
            user.Location = null;
            user.Latitude = null;
            user.Longitude = null;
            user.ProfilePicture = null;
            user.PasswordHash = null;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.AnonymizeAtUtc = null;
            database.AuditLogs.Add(new AuditLog
            {
                Action = "AccountAnonymized",
                EntityType = nameof(Kullanici),
                EntityId = user.Id.ToString()
            });
        }
        if (dueUsers.Count > 0) await database.SaveChangesAsync(cancellationToken);
    }
}
