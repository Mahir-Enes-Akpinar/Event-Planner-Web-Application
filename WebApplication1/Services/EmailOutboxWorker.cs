using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class EmailOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailOutboxWorker> _logger;

    public EmailOutboxWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<EmailOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await DeliverBatch(stoppingToken); }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "Email outbox cycle failed.");
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task DeliverBatch(CancellationToken cancellationToken)
    {
        var host = _configuration["EmailSettings:SMTPHost"];
        var user = _configuration["EmailSettings:SMTPUser"];
        var password = _configuration["EmailSettings:SMTPPass"];
        var from = _configuration["EmailSettings:FromEmail"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from)) return;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<Context>();
        var messages = await database.EmailOutbox
            .Where(item => item.SentAtUtc == null && item.AttemptCount < 8 && item.NextAttemptAtUtc <= DateTime.UtcNow)
            .OrderBy(item => item.Id).Take(20).ToListAsync(cancellationToken);

        foreach (var item in messages)
        {
            try
            {
                using var client = new SmtpClient(host, _configuration.GetValue("EmailSettings:SMTPPort", 587))
                {
                    Credentials = new NetworkCredential(user, password), EnableSsl = true
                };
                using var message = new MailMessage(from, item.Recipient, item.Subject, item.Body);
                await client.SendMailAsync(message, cancellationToken);
                item.SentAtUtc = DateTime.UtcNow;
                item.LastError = null;
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                item.AttemptCount++;
                item.NextAttemptAtUtc = DateTime.UtcNow.AddMinutes(Math.Pow(2, item.AttemptCount));
                item.LastError = exception.Message[..Math.Min(exception.Message.Length, 1000)];
                _logger.LogWarning(exception, "Email outbox item {OutboxId} delivery failed.", item.Id);
            }
        }
        if (messages.Count > 0) await database.SaveChangesAsync(cancellationToken);
    }
}
