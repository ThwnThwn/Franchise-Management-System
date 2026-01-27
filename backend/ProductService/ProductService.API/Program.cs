using System.Reflection;
using AutoMapper;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using ProductService.API.Middleware;
using ProductService.API.Services;
using ProductService.Application.Mapping;
using ProductService.Application.Repositories;
using ProductService.Infrastructure.ApplicationDbContext;
using ProductService.Infrastructure.Extensions;
using ProductService.Infrastructure.Interceptor;
using ProductService.Infrastructure.Repositories;

namespace ProductService.API
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
            builder.Services.AddGrpcReflection(); // Enable gRPC Server Reflection
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
            
            // EF Core interceptors used by ProductDbContext
            builder.Services.AddScoped<UpdateTimestampInterceptor>();
            
            // DbContext + wire interceptor via DI
            builder.Services.AddDbContext<ProductDbContext>(
                (sp, opt) =>
                {
                    opt.UseNpgsql(connectionString);
                    opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
                });
            
            // Expose DbContext via abstraction used by repositories
            builder.Services.AddScoped<IProductDbContext>(sp => sp.GetRequiredService<ProductDbContext>());

            // Configure AutoMapper
            // Auto scan all Profile classes from API and Application assemblies
            var apiAssembly = Assembly.GetExecutingAssembly();
            var applicationAssembly = typeof(ProductMappingProfile).Assembly;

            builder.Services.AddSingleton(_ =>
            {
                var cfg = new MapperConfiguration(c =>
                {
                    c.AddMaps(apiAssembly);           // Scan API assembly (ProductGrpcMappingProfile)
                    c.AddMaps(applicationAssembly);    // Scan Application assembly (ProductMappingProfile)
                });
                return cfg.CreateMapper();
            });

            // Register Application services (MediatR, FluentValidation, ValidationBehavior)
            builder.Services.AddApplication();

            // Register repositories
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
                });
            }

            app.UseCors();

            // Add global exception handler middleware
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<ProductGrpcService>();
            app.MapGrpcService<Services.CategoryGrpcService>();
            app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
            app.MapGet("/", () => "Product Service API is running. Visit /swagger for API documentation.");

            app.Run();
        }
    }
}