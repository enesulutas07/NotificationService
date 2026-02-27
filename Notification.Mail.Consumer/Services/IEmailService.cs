using System.Threading;
using System.Threading.Tasks;

namespace Notification.Mail.Consumer.Services;

public interface IEmailService
{
    Task SendOrderCreatedEmailAsync(Guid orderId, Guid userId, string email, CancellationToken cancellationToken = default);
}

