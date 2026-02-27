using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Notification.Mail.Consumer.Consumers;
using Notification.Data;
using Notification.Mail.Consumer.Services;

var builder = Host.CreateApplicationBuilder(args);

var elasticUri = builder.Configuration["Serilog:Elasticsearch:Uri"] ?? "http://localhost:9200";
var indexFormat = builder.Configuration["Serilog:Elasticsearch:IndexFormat"] ?? "notification-mail-consumer-logs-{0:yyyy.MM.dd}";
var autoRegisterTemplate = builder.Configuration.GetValue("Serilog:Elasticsearch:AutoRegisterTemplate", true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
    {
        AutoRegisterTemplate = autoRegisterTemplate,
        IndexFormat = indexFormat
    })
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: false);

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmailService, SmtpEmailService>();

var inboxDuplicateDetectionMinutes = builder.Configuration.GetValue("MassTransit:Inbox:DuplicateDetectionWindowMinutes", 5);
var inboxQueryDelaySeconds = builder.Configuration.GetValue("MassTransit:Inbox:QueryDelaySeconds", 60);
var disableInboxCleanup = builder.Configuration.GetValue("MassTransit:Inbox:DisableCleanupService", false);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<NotificationDbContext>(o =>
    {
        o.UsePostgres();
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(inboxDuplicateDetectionMinutes);
        o.QueryDelay = TimeSpan.FromSeconds(inboxQueryDelaySeconds);
        if (disableInboxCleanup)
            o.DisableInboxCleanupService();
    });
    x.AddConfigureEndpointsCallback((context, name, endpointCfg) =>
        endpointCfg.UseEntityFrameworkOutbox<NotificationDbContext>(context));

    x.AddConsumer<OrderCreatedMailConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitUrl = builder.Configuration["RabbitMQ:Url"] ?? "amqp://guest:guest@localhost:5672/";
        cfg.Host(new Uri(rabbitUrl));

        var queuePrefix = builder.Configuration["RabbitMQ:QueuePrefix"] ?? "NotificationMail";
        cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(queuePrefix, includeNamespace: false));

        var retryLimit = builder.Configuration.GetValue("MessageRetry:MaxRetryCount", 5);
        var minInterval = TimeSpan.FromSeconds(builder.Configuration.GetValue("MessageRetry:MinIntervalSeconds", 1));
        var maxInterval = TimeSpan.FromMinutes(builder.Configuration.GetValue("MessageRetry:MaxIntervalMinutes", 5));
        var intervalDelta = TimeSpan.FromSeconds(builder.Configuration.GetValue("MessageRetry:IntervalDeltaSeconds", 2));
        cfg.UseMessageRetry(r => r.Exponential(retryLimit, minInterval, maxInterval, intervalDelta));
    });
});

var host = builder.Build();
try
{
    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await db.Database.MigrateAsync();
    }

    host.Run();
}
finally
{
    Log.CloseAndFlush();
}

