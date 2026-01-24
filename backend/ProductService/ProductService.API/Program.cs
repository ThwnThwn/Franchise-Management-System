using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.API.Mapping;
using ProductService.API.Services;
using ProductService.Application.Interfaces;
using ProductService.Application.Mapping;
using ProductService.Application.Services;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;

namespace ProductService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();

            // Add REST API controllers
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

            // Configure database context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Configure AutoMapper (REQUIRED - Services use IMapper)
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

            // Register repositories
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            // Register services
            builder.Services.AddScoped<ProductApplicationService>();

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

            // Seed database (only in Development)
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ProductDbContext>();
                        await DbInitializer.SeedAsync(context);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while seeding the database.");
                    }
                }
            }

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