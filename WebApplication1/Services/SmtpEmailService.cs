using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly Context _database;
    public SmtpEmailService(Context database) => _database = database;

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        _database.EmailOutbox.Add(new EmailOutboxMessage
        {
            Recipient = recipient,
            Subject = subject,
            Body = body
        });
        await _database.SaveChangesAsync(cancellationToken);
    }
}
