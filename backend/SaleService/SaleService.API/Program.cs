using AutoMapper;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SalesService.API.Middleware;
using SalesService.API.Services.Orders;
using SalesService.API.Services.Products;
using SalesService.API.Services.Payments;
using SalesService.API.Middleware;
using SalesService.Application.Interfaces.Orders;
using SalesService.Application.Interfaces.Products;
using SalesService.Infrastructure.Extensions;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Infrastructure.Repositories.Orders;
using SalesService.Infrastructure.Repositories.Payments;
using SalesService.Infrastructure.ApplicationDbContext;
using SalesService.Infrastructure.Extensions;
using SalesService.Infrastructure.Interceptor;
using SalesService.Infrastructure.Models;
using SalesService.Infrastructure.Repositories.Orders;
using SaleService.Application.ExternalServices;
using SaleService.Infrastructure.ExternalServices;
using SalesService.API.Services.Identity;

namespace SalesService.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Load .env file
        Env.Load("../../../.env");

        var builder = WebApplication.CreateBuilder(args);

        // Add environment variables to configuration
        builder.Configuration.AddEnvironmentVariables();

        // Configure Kestrel for gRPC (HTTPS + HTTP/2)
        var saleServicePort = int.Parse(builder.Configuration["SALES_PORT"] ?? "5003");
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(saleServicePort, listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
                listenOptions.UseHttps(); // Development certificate
            });
        });

        // Add services to the container.
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<Interceptors.GrpcExceptionInterceptor>();
        });
        builder.Services.AddGrpcReflection(); // Enable gRPC Server Reflection
        builder.Services.AddControllers();

        // Add API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Sale Service API",
                Version = "v1",
                Description = "API quản lý đơn hàng và bán hàng"
            });
        });

        // Interceptors
        builder.Services.AddScoped<UpdateTimestampInterceptor>();

        // Connect Db
        var connectionString = builder.Configuration["SALES_DB_CONNECTION"];

        // EF Core interceptors used by SalesDbContext
        builder.Services.AddScoped<UpdateTimestampInterceptor>();

        // DbContext + wire interceptor via DI
        builder.Services.AddDbContext<SalesDbContext>(
            (sp, opt) =>
            {
                opt.UseNpgsql(connectionString);
                opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
            });

        // Expose DbContext via abstraction used by repositories
        builder.Services.AddScoped<ISalesDbContext>(sp => sp.GetRequiredService<SalesDbContext>());

        // KAFKA - Override for local development
        var kafkaBootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
        if (kafkaBootstrap.Contains("kafka:"))
        {
            kafkaBootstrap = "localhost:9092";  // Use localhost when running outside Docker
        }
        builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] = kafkaBootstrap;

        // Configure AutoMapper
        var applicationAssembly = typeof(OrderMappingProfile).Assembly;

        builder.Services.AddSingleton(_ =>
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(applicationAssembly);
            });
            return cfg.CreateMapper();
        });

        // Register Application services (MediatR, FluentValidation, ValidationBehavior)
        builder.Services.AddApplication();

        // Register repositories
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Register ProductService gRPC client
        builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();

        // Register IdentityService gRPC client
        builder.Services.AddSingleton<IIdentityServiceClient, IdentityServiceClient>();

        // Register Kafka Producer
        builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

        // Register GlobalExceptionHandlerMiddleware
        builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sale Service API v1");
            });
        }

        app.UseCors();

        // Add global exception handler middleware
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthorization();

        app.MapControllers();
        app.MapGrpcService<OrderGrpcService>();
        app.MapGrpcService<PaymentGrpcService>();
        app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
        app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");

        app.Run();
    }
}