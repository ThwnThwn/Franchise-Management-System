using DotNetEnv;
using FluentValidation;
using FranchiseService.API.Interceptors;
using FranchiseService.Application.Behaviors;
using FranchiseService.Application.Features.Queries.CheckStoreAvailability;
using FranchiseService.Infrastructure.Extentions;
using MediatR;

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
            var servicePort = int.Parse(builder.Configuration["FRANCHISE_PORT"] ?? "5006");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(servicePort, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                    listenOptions.UseHttps();
                });
            });

            // ========================================================
            // 3. ĐĂNG KÝ SERVICES (DEPENDENCY INJECTION)
            // ========================================================

            // Database & Infrastructure
            var connectionString = builder.Configuration["FRANCHISE_DB_CONNECTION"]
                ?? "Host=localhost;Port=5432;Database=franchise_db;Username=admin;Password=12345";
            builder.Services.AddInfrastructure(connectionString);

            // MediatR (v11)
            builder.Services.AddMediatR(typeof(CheckStoreAvailabilityQuery));

            // FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(CheckStoreAvailabilityQuery).Assembly);

            // Pipeline Behavior (Validation)
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // gRPC & Interceptor (GỘP LẠI Ở ĐÂY)
            // Bước A: Đăng ký class Interceptor vào DI Container
            builder.Services.AddSingleton<GrpcExceptionInterceptor>();

            // Bước B: Config gRPC và gắn Interceptor
            builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcExceptionInterceptor>();
                options.EnableDetailedErrors = true;
            });
            builder.Services.AddGrpcReflection();

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
            app.MapGrpcService<Services.FranchiseGrpcService>(); // Class đã bỏ static
            app.MapGrpcReflectionService();

            // Root Endpoint (Health check nhẹ)
            app.MapGet("/", () => $"Franchise Service is running on port {servicePort}. Environment: {builder.Environment.EnvironmentName}");

            app.Run();
        }
    }
}