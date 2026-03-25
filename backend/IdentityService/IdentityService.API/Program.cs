using DotNetEnv;
using IdentityService.API.Extensions;
using IdentityService.API.Interceptors;
using IdentityService.API.Services;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppConfigurations;
using IdentityService.Application.DependencyInjection;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Infrastructure.DependencyInjection;
using IdentityService.Infrastructure.ExternalServices;
using IdentityService.Infrastructure.Persistences.Cache;
using IdentityService.Infrastructure.Settings;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;

namespace IdentityService.API
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            // --- CONFIGURATION ---
            Env.Load("../../../.env");
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();
            // -- SMTP EMAIL ---
            builder.Services.Configure<OtpConfigs>(options =>
            {
                options.OtpTtl = int.Parse(builder.Configuration["OTP_TTL"] ?? "10"); // Default 10 minutes
            });
            builder.Services.Configure<SmtpSettings>(opt =>
            {
                opt.Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
                opt.Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
                opt.Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
                opt.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
                opt.EnableSsl = bool.Parse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL") ?? "true");
                opt.FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "";
                opt.FromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "CFMS";
            });
            // --- GRPC & CORE ---
            builder.Services.AddGrpc(options => options.Interceptors.Add<GrpcExceptionInterceptor>());
            builder.Services.AddGrpcReflection();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // --- IDENTITY & AUTH (Using Extension) ---
            builder.Services.AddJwtIdentity(builder.Configuration);
            builder.Services.AddAuthorization();

            // --- DATABASE & REDIS ---
            var dbConn = builder.Configuration["IDENTITY_DB_CONNECTION"]
                ?? "Host=localhost;Port=5432;Database=identity_db;Username=admin;Password=12345";
            var redisConn = builder.Configuration["REDIS_CONFIGURATION"]
                ?? "localhost:6379,abortConnect=false";
            var blackListDb = builder.Configuration["REDIS_BLACKLIST_DB"] ?? "0";
            var permDb = builder.Configuration["REDIS_PERM_DB"] ?? "1";

            builder.Services.AddInfrastructure(dbConn, redisConn, blackListDb, permDb);
            //builder.Services.AddStackExchangeRedisCache(opt =>
            //{
            //    opt.Configuration = redisConn + ",defaultDatabase=1";
            //    opt.InstanceName = "chb_redis";
            //});

            // Cloudinary configuration
            builder.Services.Configure<CloudinarySettings>(options =>
            {
                options.CloudName = builder.Configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty;
                options.ApiKey = builder.Configuration["CLOUDINARY_API_KEY"] ?? string.Empty;
                options.ApiSecret = builder.Configuration["CLOUDINARY_API_SECRET"] ?? string.Empty;
            });

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

            // --- APP SERVICES ---
            builder.Services.AddApplication();
            builder.Services.AddScoped<ITokenBlacklist, TokenBlacklist>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IEngagementServiceClient, EngagementServiceClient>();
            builder.Services.AddScoped<ISaleServiceClient, SaleServiceClient>();
            // --- SERVER CONFIG ---
            var servicePort = int.Parse(builder.Configuration["IDENTITY_PORT"] ?? "5001");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });

            var app = builder.Build();

            // --- PIPELINE ---
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<AuthGrpcService>();
            app.MapGrpcService<UserGrpcService>();
            app.MapGrpcService<RoleGrpcService>();
            app.MapGrpcService<RolePermissionGrpcService>();
            app.MapGrpcReflectionService();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Lấy service ra từ Container
                    var syncService = services.GetRequiredService<IPermissionSyncService>();

                    // Gọi hàm Sync
                    Console.WriteLine("Start syncing permissions...");
                    await syncService.SyncAllAsync();
                    Console.WriteLine("Permissions synced successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while syncing permissions: " + ex.Message);
                }
            }
            app.MapGet("/", () => $"Identity Service is running on port {servicePort}. Environment: {builder.Environment.EnvironmentName}");
            app.Run();
        }
    }
}