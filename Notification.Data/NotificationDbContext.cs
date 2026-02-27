using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notification.Entity;

namespace Notification.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<EmailNotification> EmailNotifications => Set<EmailNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EmailNotification>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CorrelationId).IsRequired();
            entity.Property(x => x.OrderId).IsRequired();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.To).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Subject).IsRequired().HasMaxLength(512);
            entity.Property(x => x.Body).IsRequired();
            entity.Property(x => x.Provider).HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.SentAt);
            entity.Property(x => x.IsSuccess).IsRequired();
            entity.Property(x => x.ErrorMessage);
        });

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}

