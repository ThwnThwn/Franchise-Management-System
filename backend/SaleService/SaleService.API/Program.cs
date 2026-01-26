using AutoMapper;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SalesService.API.Services.Orders;
using SalesService.API.Services.Products;
using SalesService.API.Middleware;
using SalesService.Infrastructure.Repositories.Orders;
using SalesService.Infrastructure.Extensions;
using SalesService.Application.Interfaces.Orders;
using SalesService.Application.Interfaces.Products;
using SalesService.Application.Mapping.Orders;
using SalesService.Infrastructure.ApplicationDbContext;
using SalesService.Infrastructure.Models;
using SalesService.Infrastructure.Interceptor;

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
        builder.Services.AddDbContext<ISalesDbContext, SalesDbContext>(options =>
            options.UseNpgsql(connectionString));

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

        // Register ProductService gRPC client
        builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();

        // Register GlobalExceptionHandlerMiddleware
        builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

        // Register gRPC Exception Interceptor
        builder.Services.AddSingleton<Interceptors.GrpcExceptionInterceptor>();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
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
        app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");

        app.Run();
    }
}