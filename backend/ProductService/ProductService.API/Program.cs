using System.Reflection;
using AutoMapper;
using Ardalis.Result;
using DotNetEnv;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.API.Mapping;
using ProductService.API.Services;
using ProductService.Application.Features.Products.GetById;
using ProductService.Application.Features.Products.GetList;
using ProductService.Application.Interfaces;
using ProductService.Application.Mapping;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;

namespace ProductService
{
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
            var productServicePort = int.Parse(builder.Configuration["PRODUCTS_PORT"] ?? "5002");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(productServicePort, listenOptions =>
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
                    Title = "Product Service API",
                    Version = "v1",
                    Description = "API quản lý sản phẩm, size và giá"
                });
            });

            // Connect Db
            var connectionString = builder.Configuration["PRODUCT_DB_CONNECTION"];

            builder.Services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register interface
            builder.Services.AddScoped<IProductDbContext>(provider =>
                provider.GetRequiredService<ProductDbContext>());


            // Configure AutoMapper
            // Auto scan all Profile classes from API and Application assemblies
            var apiAssembly = Assembly.GetExecutingAssembly();
            var applicationAssembly = typeof(ProductMappingProfile).Assembly;

            builder.Services.AddSingleton<IMapper>(_ =>
            {
                var cfg = new MapperConfiguration(c =>
                {
                    c.AddMaps(apiAssembly);           // Scan API assembly (ProductGrpcMappingProfile)
                    c.AddMaps(applicationAssembly);    // Scan Application assembly (ProductMappingProfile)
                });
                return cfg.CreateMapper();
            });

            // Register MediatR (scan Application assembly)
            builder.Services.AddMediatR(typeof(GetProductByIdQuery).Assembly);

            // Register repositories
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
                });
            }

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<GreeterService>();
            app.MapGrpcService<ProductGrpcService>();
            app.MapGet("/", () => "Product Service API is running. Visit /swagger for API documentation.");

            app.Run();
        }
    }
}