using ECommerce.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Mail.Service;

namespace Notification.Mail.Consumer.Consumers;

public class OrderCreatedMailConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedMailConsumer> _logger;

    public OrderCreatedMailConsumer(IEmailService emailService, ILogger<OrderCreatedMailConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        await _emailService.SendOrderCreatedEmailAsync(message.OrderId, message.UserId, message.Email, context.CancellationToken);
        
        _logger.LogInformation("OrderCreatedEvent mail bildirimi için alındı. OrderId: {OrderId}, UserId: {UserId}, Email: {Email}, Phone: {PhoneNumber}, CorrelationId: {CorrelationId}",
            message.OrderId, message.UserId, message.Email, message.PhoneNumber, context.CorrelationId);
    }
}

