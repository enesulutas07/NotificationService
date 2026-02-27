namespace Notification.Entity;

public class EmailNotification
{
    public int Id { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string To { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string? Provider { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

