using DotNetEnv;
using FluentValidation;
using FranchiseService.API.Interceptors;
using FranchiseService.Application.Behaviors;
using FranchiseService.Application.Features.Stores.Queries.CheckStoreAvailability;
using FranchiseService.Infrastructure.Extentions;
using MediatR;

using FranchiseService.API.BackgroundJobs;
using FranchiseService.Application.Services;
using IdentityService.Grpc.Protos.User;

namespace FranchiseService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 1. Load Environment Variables
            Env.Load("../../../.env");

            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();

            // 2. Configure Kestrel (HTTP/2 for gRPC)
            //var servicePort = int.Parse(builder.Configuration["FRANCHISE_PORT"] ?? "5006");

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ConfigureEndpointDefaults(listenOptions =>
            //    {
            //        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            //    });
            //});

            var servicePort = int.Parse(builder.Configuration["FRANCHISE_PORT"] ?? "5006");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(servicePort, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });

                options.ListenAnyIP(5016, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
                });
            });

            // ========================================================
            // 3. ĐĂNG KÝ SERVICES (DEPENDENCY INJECTION)
            // ========================================================

            // Database & Infrastructure
            var connectionString = builder.Configuration["FRANCHISE_DB_CONNECTION"]
                ?? "Host=localhost;Port=5432;Database=franchise_db;Username=admin;Password=12345";
            builder.Services.AddInfrastructure(connectionString, builder.Configuration);

            // MediatR (v11)
            builder.Services.AddMediatR(typeof(CheckStoreAvailabilityQuery));

            // FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(CheckStoreAvailabilityQuery).Assembly);

            // Pipeline Behavior (Validation)
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // gRPC & Interceptor (GỘP LẠI Ở ĐÂY)
            // Bước A: Đăng ký class Interceptor vào DI Container
            // Bước A: Đăng ký class Interceptor vào DI Container
            builder.Services.AddSingleton<GrpcExceptionInterceptor>();

            // Worker (Background Job)
            builder.Services.AddHostedService<OutboxWorker>();

            // Bước B: Config gRPC và gắn Interceptor
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcExceptionInterceptor>();
                options.EnableDetailedErrors = true;
            });
            builder.Services.AddGrpcReflection();

            // HttpContextAccessor (de forward x-user-id header sang gRPC metadata)
            builder.Services.AddHttpContextAccessor();

            // gRPC Client → IdentityService (để validate user và lấy thông tin nhân viên)
            // Scoped vi can doc HttpContext.Request.Headers cua tung request
            builder.Services.AddScoped<Application.ClientInterfaces.IIdentityServiceClient,
                Infrastructure.Clients.Identity.IdentityServiceClient>();

            builder.Services.AddScoped<IShiftValidationService, ShiftValidationService>();
            builder.Services.AddScoped<IShiftRuleService, ShiftRuleService>();
            builder.Services.AddScoped<IOvertimeCalculationService, OvertimeCalculationService>();
            builder.Services.AddScoped<IShiftApplicationAutoRejectService, ShiftApplicationAutoRejectService>();
            builder.Services.AddScoped<FranchiseService.Application.Features.ShiftSwaps.Helpers.SwapValidationHelper>();


            // Rest API Controllers
            builder.Services.AddControllers();

            // CORS
            var frontEndOrigin = builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:3000";
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithOrigins(frontEndOrigin)
                          .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
                });
            });


            //GRPC CLIENT
            builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
            {
                var url = builder.Configuration["IDENTITY_SERVICE_URL"];
                if (string.IsNullOrEmpty(url))
                {
                    // Nếu không tìm thấy trong ENV/Appsettings, báo lỗi ngay khi khởi động
                    throw new InvalidOperationException("Missing configuration: IDENTITY_SERVICE_URL");
                }
                o.Address = new Uri(url);
            });

            // ========================================================
            // 4. BUILD & CONFIGURE PIPELINE
            // ========================================================
            var app = builder.Build();

            // Middleware Pipeline
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization(); // Mặc dù chưa có Authen nhưng cứ để đây giữ chỗ

            // Map Endpoints
            app.MapControllers();

            // Map gRPC Services
            app.MapGrpcService<Services.StoreGrpcService>();
            app.MapGrpcService<Services.ShiftGrpcService>();
            app.MapGrpcService<Services.AvailabilityGrpcService>();
            app.MapGrpcService<Services.ShiftRuleGrpcService>();
            app.MapGrpcService<Services.ShiftTemplateGrpcService>();
            app.MapGrpcService<Services.ShiftApplicationGrpcService>();
            app.MapGrpcService<Services.ShiftSwapGrpcService>();
            app.MapGrpcService<Services.FranchiseGrpcService>();
            app.MapGrpcReflectionService();


            // Root Endpoint (Health check nhẹ)
            app.MapGet("/", () => $"Franchise Service is running on port {servicePort}. Environment: {builder.Environment.EnvironmentName}");

            app.Run();
        }
    }
}