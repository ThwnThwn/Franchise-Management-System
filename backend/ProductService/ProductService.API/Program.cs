using DotNetEnv;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.API.Middleware;
using ProductService.API.Services;
using ProductService.Application.Extentions;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using ProductService.Infrastructure.ApplicationDbContext;
using ProductService.Infrastructure.Extensions;
using ProductService.Infrastructure.Interceptor;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Services;
using ProductService.Infrastructure.Settings;

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
            //var productServicePort = int.Parse(builder.Configuration["PRODUCTS_PORT"] ?? "5002");
            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ListenLocalhost(productServicePort, listenOptions =>
            //    {
            //        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            //        listenOptions.UseHttps(); // Development certificate
            //    });
            //});
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
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

            builder.Services.AddInfrastructures(builder.Configuration);
            builder.Services.AddApplication();

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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
                });
            }

            app.UseRouting();

            app.UseCors();
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<ProductGrpcService>();
            app.MapGrpcService<Services.CategoryGrpcService>();
            app.MapGrpcService<Services.ProductVariantInternalGrpcService>();
            app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
            app.MapGet("/", () => "Product Service API is running. Visit /swagger for API documentation.");

            app.Run();
        }
    }
}