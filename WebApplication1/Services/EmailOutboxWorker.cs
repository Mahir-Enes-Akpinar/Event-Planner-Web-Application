using System.Net;
using System.Net.Mail;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class EmailOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailOutboxWorker> _logger;

    public EmailOutboxWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<EmailOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
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
        var from = _configuration["EmailSettings:FromEmail"];
        if (string.IsNullOrWhiteSpace(from)) return;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<Context>();
        var messages = await database.EmailOutbox
            .Where(item => item.SentAtUtc == null && item.AttemptCount < 8 && item.NextAttemptAtUtc <= DateTime.UtcNow)
            .OrderBy(item => item.Id).Take(20).ToListAsync(cancellationToken);

        foreach (var item in messages)
        {
            try
            {
                if (UseResend())
                    await SendWithResendAsync(from, item, cancellationToken);
                else
                    await SendWithSmtpAsync(from, item, cancellationToken);

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

    private bool UseResend()
    {
        var provider = _configuration["EmailProvider"];
        var apiKey = _configuration["Resend:ApiKey"];
        return provider?.Equals("Resend", StringComparison.OrdinalIgnoreCase) == true ||
               !string.IsNullOrWhiteSpace(apiKey);
    }

    private async Task SendWithResendAsync(string from, EmailOutboxMessage item, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Resend:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Resend API key is not configured.");

        var baseUrl = _configuration["Resend:BaseUrl"] ?? "https://api.resend.com/";
        var client = _httpClientFactory.CreateClient(nameof(EmailOutboxWorker));
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var fromName = _configuration["EmailSettings:FromName"];
        var fromAddress = string.IsNullOrWhiteSpace(fromName) ? from : $"{fromName} <{from}>";
        var payload = new ResendEmailRequest(fromAddress, [item.Recipient], item.Subject, item.Body);
        using var response = await client.PostAsJsonAsync("emails", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Resend returned {(int)response.StatusCode}: {error}");
        }
    }

    private async Task SendWithSmtpAsync(string from, EmailOutboxMessage item, CancellationToken cancellationToken)
    {
        var host = _configuration["EmailSettings:SMTPHost"];
        var user = _configuration["EmailSettings:SMTPUser"];
        var password = _configuration["EmailSettings:SMTPPass"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SMTP settings are not configured.");

        using var client = new SmtpClient(host, _configuration.GetValue("EmailSettings:SMTPPort", 587))
        {
            Credentials = new NetworkCredential(user, password), EnableSsl = true
        };
        using var message = new MailMessage(from, item.Recipient, item.Subject, item.Body);
        await client.SendMailAsync(message, cancellationToken);
    }

    private sealed record ResendEmailRequest(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] string[] To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("text")] string Text);
}
