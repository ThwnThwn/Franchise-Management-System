using DotNetEnv;
using IdentityService.API.Extensions;
using IdentityService.API.Interceptors;
using IdentityService.API.Services;
using IdentityService.Application.Abstrations;
using IdentityService.Application.AppConfigurations;
using IdentityService.Application.DependencyInjection;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.DependencyInjection;
using IdentityService.Infrastructure.Persistences.Cache;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace IdentityService.API
{
    public class Program
    {
        private static void Main(string[] args)
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
            var dbConn = builder.Configuration["IDENTITY_DB_CONN"]
                ?? "Host=localhost;Port=5432;Database=identity_db;Username=admin;Password=12345";
            builder.Services.AddInfrastructure(dbConn);

            var redisConn = builder.Configuration["REDIS_CONFIGURATION"]
                ?? "localhost:6379,abortConnect=false";
            builder.Services.AddStackExchangeRedisCache(opt =>
            {
                opt.Configuration = redisConn;
                opt.InstanceName = "chb_redis";
            });
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));

            // KAFKA - Override for local development
            var kafkaBootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
            if (kafkaBootstrap.Contains("kafka:"))
            {
                kafkaBootstrap = "localhost:9092";  // Use localhost when running outside Docker
            }
            builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] = kafkaBootstrap;

            // --- APP SERVICES ---
            builder.Services.AddApplication();
            builder.Services.AddScoped<ITokenBlacklist, TokenBlacklist>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            // --- SERVER CONFIG ---
            var servicePort = int.Parse(builder.Configuration["IDENTITY_PORT"] ?? "5001");
            builder.WebHost.ConfigureKestrel(opt =>
                opt.ListenAnyIP(servicePort, l => l.Protocols = HttpProtocols.Http2));

            var app = builder.Build();

            // --- PIPELINE ---
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGrpcService<AuthGrpcService>();
            app.MapGrpcService<UserGrpcService>();
            app.MapGrpcService<RoleGrpcService>();
            app.MapGrpcReflectionService();

            app.Run();
        }
    }
}