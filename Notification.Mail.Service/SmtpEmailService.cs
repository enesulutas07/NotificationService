using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notification.Data;
using Notification.Entity;
using System.Net;
using System.Net.Mail;

namespace Notification.Mail.Service;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly NotificationDbContext _dbContext;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, NotificationDbContext dbContext, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SendOrderCreatedEmailAsync(Guid orderId, Guid userId, string email, CancellationToken cancellationToken = default)
    {
        var smtpSection = _configuration.GetSection("Smtp");
        var host = smtpSection.GetValue<string>("Host");
        var port = smtpSection.GetValue<int>("Port");
        var enableSsl = smtpSection.GetValue("EnableSsl", false);
        var userName = smtpSection.GetValue<string>("UserName");
        var password = smtpSection.GetValue<string>("Password");
        var fromAddress = smtpSection.GetValue<string>("FromAddress") ?? throw new InvalidOperationException("Smtp:FromAddress configuration is required.");

        var toAddress = email;

        var subject = $"Siparişiniz oluşturuldu - {orderId}";
        var body = $"Merhaba,\n\n{orderId} numaralı siparişiniz başarıyla oluşturuldu.\n\nTeşekkürler.";

        var notification = new EmailNotification
        {
            CorrelationId = orderId,
            OrderId = orderId,
            UserId = userId,
            To = toAddress,
            Subject = subject,
            Body = body,
            Provider = host,
            CreatedAt = DateTime.UtcNow,
            IsSuccess = false
        };

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            client.Credentials = new NetworkCredential(userName, password);

            var message = new MailMessage(fromAddress, toAddress, subject, body);

            await client.SendMailAsync(message, cancellationToken);

            notification.IsSuccess = true;
            notification.SentAt = DateTime.UtcNow;
            _dbContext.EmailNotifications.Add(notification);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OrderCreated email gönderimi başarısız oldu. OrderId: {OrderId}, UserId: {UserId}, Email: {Email}", orderId, userId, email);
            throw;
        }
    }
}
