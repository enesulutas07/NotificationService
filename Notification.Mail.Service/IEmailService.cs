namespace Notification.Mail.Service;

public interface IEmailService
{
    Task SendOrderCreatedEmailAsync(Guid orderId, Guid userId, string email, CancellationToken cancellationToken = default);
}
