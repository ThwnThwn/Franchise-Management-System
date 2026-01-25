using System.Reflection;
using AutoMapper;
using DotNetEnv;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaleService.Application.Features.Orders.Create;
using SaleService.Application.Interfaces;
using SaleService.Application.Mapping;
using SaleService.API.Services.Orders;
using SaleService.API.Services.Products;
using SaleService.Infrastructure.Data;
using SaleService.Infrastructure.Repositories.Orders;

namespace SaleService;

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
        builder.Services.AddGrpc();
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

        // Connect Db
        var connectionString = builder.Configuration["SALE_DB_CONNECTION"];

        builder.Services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register interface
        builder.Services.AddScoped<ISalesDbContext>(provider =>
            provider.GetRequiredService<SalesDbContext>());

        // Configure AutoMapper
        var applicationAssembly = typeof(Application.Mapping.Orders.OrderMappingProfile).Assembly;

        builder.Services.AddSingleton<IMapper>(_ =>
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(applicationAssembly);
            });
            return cfg.CreateMapper();
        });

        // Register MediatR (scan Application assembly)
        builder.Services.AddMediatR(typeof(CreateOrderCommand).Assembly);

        // Register repositories
        builder.Services.AddScoped<Application.Interfaces.Orders.IOrderRepository, Infrastructure.Repositories.Orders.OrderRepository>();

        // Register ProductService gRPC client
        builder.Services.AddSingleton<Application.Interfaces.Products.IProductServiceClient, API.Services.Products.ProductServiceClient>();

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

        app.UseAuthorization();

        app.MapControllers();
        app.MapGrpcService<SaleService.API.Services.Orders.OrderGrpcService>();
        app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");

        app.Run();
    }
}
