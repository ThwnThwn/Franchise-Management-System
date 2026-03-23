using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions;
using NotificationService.Application.ExternalServices;
using NotificationService.Infrastructure.Configurations;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;
using SharedKernel.Infrastructure.Kafka;

// Load environment variables
Env.Load("../../../.env");
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
// DATABASE
var dbConn = builder.Configuration["NOTIFICATION_DB_CONNECTION"]
    ?? "Host=localhost;Port=5432;Database=notification_db;Username=admin;Password=12345";
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(dbConn));

// KAFKA - Override for local development
var kafkaBootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") 
                     ?? builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] 
                     ?? "localhost:9092";

bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!isDocker)
{
    kafkaBootstrap = "localhost:9092";
}
builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] = kafkaBootstrap;

// SMTP SETTINGS
builder.Services.Configure<SmtpSettings>(opt =>
{
    opt.Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
    opt.Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
    opt.Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
    opt.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
    opt.EnableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true");
    opt.FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "";
    opt.FromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "CFMS Notification";
});
// DEPENDENCY INJECTION
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IIdentityServiceClient, NotificationService.Infrastructure.ExternalServices.IdentityServiceClient>();

// KAFKA CONSUMER (Generic)
builder.Services.AddKafkaConsumer(config =>
{
    config.Register<SharedKernel.Events.OtpRequestedEvent>(SharedKernel.Constants.KafkaTopics.OtpNotifications, typeof(NotificationService.Application.EventHandlers.OtpNotificationHandler));
    config.Register<SharedKernel.Events.OrderStatusEvent>(SharedKernel.Constants.KafkaTopics.OrderNotifications, typeof(NotificationService.Application.EventHandlers.OrderStatusHandler));
    config.Register<SharedKernel.Events.LowStockEvent>(SharedKernel.Constants.KafkaTopics.LowStockNotifications, typeof(NotificationService.Application.EventHandlers.LowStockHandler));
    config.Register<SharedKernel.Events.PasswordResetRequestedEvent>(SharedKernel.Constants.KafkaTopics.PasswordResetNotifications, typeof(NotificationService.Application.EventHandlers.PasswordResetHandler));
});
// LOGGING
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
var app = builder.Build();
// AUTO MIGRATION
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}
app.MapGet("/", () => "NotificationService is running!");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "NotificationService" }));
app.Run();