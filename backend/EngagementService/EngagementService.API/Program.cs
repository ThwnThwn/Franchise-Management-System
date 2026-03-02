

using DotNetEnv;
using EngagementService.API.Services.LoyaltyAccount;
using EngagementService.API.Services.MemberShipTier;
using EngagementService.Application.Abstraction;
using EngagementService.Application.ClientInterface;
using EngagementService.Application.DependencyInjection;
using EngagementService.Application.Persistences;
using EngagementService.Domain.Constants;
using EngagementService.Infrastucture.AppDbContext;
using EngagementService.Infrastucture.DependencyInjection;
using EngagementService.Infrastucture.Interceptor;
using EngagementService.Infrastucture.Persistences;
using EngagementService.API.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.NameTranslation; // Thêm namespace này
using QuestPDF.Infrastructure;
using SharedKernel.Infrastructure.Kafka;
using EngagementService.Grpc.Protos.LoyaltyAccountGrpc;
using EngagementService.Grpc.Protos.MembershipTier;
using EngagementService.Grpc.Protos.PointLogGrpc;
using EngagementService.API.Services.PointLog;
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

        // 1. Khởi tạo Builder cho DataSource
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        // 2. Sử dụng NpgsqlNullNameTranslator để giữ nguyên định dạng VIẾT HOA (EARN, REDEEM...)
        var translator = new NpgsqlNullNameTranslator();

        // 3. Đăng ký các Enum với Translator này
        dataSourceBuilder.MapEnum<PointActionType>("point_action_type", translator);
        dataSourceBuilder.MapEnum<PointReasonCode>("point_reason_code", translator);
        dataSourceBuilder.MapEnum<UserVoucherStatus>("user_voucher_status", translator);
        dataSourceBuilder.MapEnum<VoucherCategory>("voucher_category", translator);
        dataSourceBuilder.MapEnum<VoucherDiscountType>("voucher_discount_type", translator);
        dataSourceBuilder.MapEnum<VoucherItemType>("voucher_item_type", translator);

        // 4. Xây dựng DataSource
        var dataSource = dataSourceBuilder.Build();

        // 5. Đăng ký DbContext sử dụng dataSource đã cấu hình
        builder.Services.AddDbContext<EngagementDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(dataSource);
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


        //Client service
        builder.Services.AddScoped<IIdentityServiceClient, IdentityServiceClient>();
        // KAFKA - Override for local development
        var kafkaBootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
        if (kafkaBootstrap.Contains("kafka:"))
        {
            kafkaBootstrap = "localhost:9092";  // Use localhost when running outside Docker
        }
        builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] = kafkaBootstrap;

        // KAFKA CONSUMER (Generic)
        builder.Services.AddKafkaConsumer(config =>
        {
            config.Register<SharedKernel.Events.UserRegisteredEvent>(SharedKernel.Constants.KafkaTopics.LoyaltyTierAccount, typeof(EngagementService.Application.EventHandler.CreateLoyaltyAccountHandler));
            config.Register<SharedKernel.Events.ProcessOrderRewardEvent>(SharedKernel.Constants.KafkaTopics.ProcessOrderReward, typeof(EngagementService.Application.EventHandler.ProcessOrderRewardHandler));
        });

        // 1. Cấu hình bản quyền QuestPDF (Bắt buộc để không bị lỗi khi chạy)
        QuestPDF.Settings.License = LicenseType.Community;

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //app.MapGrpcService<GreeterService>();
        app.UseCors();
        app.UseAuthorization();
        app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
        app.MapGrpcService<LoyaltyAccountGrpcService>();
        app.MapGrpcService<MembershipTierGrpcService>();
        app.MapGrpcService<PointLogGrpcService>();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // Lấy service ra từ Container
                var syncTierService = services.GetRequiredService<IMembershipTierSyncService>();
                var syncPointRuleService = services.GetRequiredService<IPointRuleSyncService>();
                // Gọi hàm Sync
                Console.WriteLine("Start syncing tier...");
                await syncTierService.SyncAllAsync();
                await syncPointRuleService.SyncAllAsync();
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