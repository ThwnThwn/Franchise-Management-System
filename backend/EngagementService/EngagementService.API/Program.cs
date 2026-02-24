

using DotNetEnv;
using EngagementService.Application.Abstraction;
using EngagementService.Application.DependencyInjection;
using EngagementService.Application.Persistences;
using EngagementService.Infrastucture.AppDbContext;
using EngagementService.Infrastucture.DependencyInjection;
using EngagementService.Infrastucture.Interceptor;
using EngagementService.Infrastucture.Persistences;
using Microsoft.EntityFrameworkCore;

namespace EngagementService.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load .env file
        Env.Load("../../../.env");

        var builder = WebApplication.CreateBuilder(args);

        // Add environment variables to configuration
        builder.Configuration.AddEnvironmentVariables();

        // Add services to the container.
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<Interceptors.GrpcExceptionInterceptor>();
        });
        builder.Services.AddGrpcReflection(); // Enable gRPC Server Reflection
        builder.Services.AddControllers();

        var engagementServicePort = int.Parse(builder.Configuration["ENGAGEMENT_PORT"] ?? "5007");

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(listenOptions =>
            {
                // Allow both HTTP/1.1 (REST) and HTTP/2 (gRPC)
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            });
        });

        // Interceptors
        builder.Services.AddScoped<UpdateTimestampInterceptor>();

        // Connect Db
        var connectionString = builder.Configuration["ENGAGEMENT_DB_CONNECTION"];
        // DbContext + wire interceptor via DI
        builder.Services.AddDbContext<EngagementDbContext>(
            (sp, opt) =>
            {
                opt.UseNpgsql(connectionString);
                opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
            });

        // Expose DbContext via abstraction used by repositories
        builder.Services.AddScoped<IEngagementDbContext>(sp => sp.GetRequiredService<EngagementDbContext>());
        // Register Application services (MediatR, FluentValidation, ValidationBehavior)
        builder.Services.AddApplication();

        var redisConn = builder.Configuration["REDIS_CONFIGURATION"]
                ?? "localhost:6379,abortConnect=false";
        var tierDb = builder.Configuration["REDIS_TIER_DB"] ?? "2";
        builder.Services.AddInfrastructure(
            redisConn,
            tierDb
        );

        // Register gRPC Exception Interceptor
        builder.Services.AddSingleton<Interceptors.GrpcExceptionInterceptor>();

        // Add CORS
        var frontEndOrigin = builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:3000";
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithOrigins(frontEndOrigin);
            });
        });

       

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //app.MapGrpcService<GreeterService>();
        app.UseCors();
        app.UseAuthorization();
        app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // Lấy service ra từ Container
                var syncService = services.GetRequiredService<IMembershipTierSyncService>();

                // Gọi hàm Sync
                Console.WriteLine("Start syncing tier...");
                await syncService.SyncAllAsync();
                Console.WriteLine("tier synced successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while syncing tier: " + ex.Message);
            }
        }
        app.Run();
    }
}