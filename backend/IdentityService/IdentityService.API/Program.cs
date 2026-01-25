using DotNetEnv;
using IdentityService.API.Extensions;
using IdentityService.API.Interceptors;
using IdentityService.API.Services;
using IdentityService.Application.Abstrations;
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