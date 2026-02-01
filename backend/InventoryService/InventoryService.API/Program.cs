
using AutoMapper;
using DotNetEnv;
using InventoryService.API.Middleware;
using InventoryService.API.Services.Identity;
using InventoryService.API.Services.Inventory;
using InventoryService.Application.ClientInterface.Indentity;
using InventoryService.Application.Constants;
using InventoryService.Application.DependencyInjection;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Infrastructure.ApplicationDbContext;
using InventoryService.Infrastructure.Interceptor;
using InventoryService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace InventoryService.API
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
            var inventoryServicePort = int.Parse(builder.Configuration[Common.INVENTORY_PORT] ?? "5005");
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(inventoryServicePort, listenOptions =>
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
            //builder.Services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            //    {
            //        Title = "Sale Service API",
            //        Version = "v1",
            //        Description = "API quản lý đơn hàng và bán hàng"
            //    });
            //});

            // Interceptors
            builder.Services.AddScoped<UpdateTimestampInterceptor>();

            // Connect Db
            var connectionString = builder.Configuration[Common.INVENTORY_DB_CONNECTION];

            // EF Core interceptors used by SalesDbContext
            builder.Services.AddScoped<UpdateTimestampInterceptor>();

            // DbContext + wire interceptor via DI
            builder.Services.AddDbContext<InventoryDbContext>(
                (sp, opt) =>
                {
                    opt.UseNpgsql(connectionString);
                    opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
                });

            // Expose DbContext via abstraction used by repositories
            builder.Services.AddScoped<IInventoryDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());

            // Configure AutoMapper
            //var applicationAssembly = typeof(OrderMappingProfile).Assembly;

            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            // Register Application services (MediatR, FluentValidation, ValidationBehavior)
            builder.Services.AddApplication();

            //// Register repositories
            //builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            //// Register ProductService gRPC client
            //builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();

            // Register GlobalExceptionHandlerMiddleware
            builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

            // Register gRPC Exception Interceptor
            builder.Services.AddSingleton<Interceptors.GrpcExceptionInterceptor>();

            // Add CORS
            var frontEndOrigin = builder.Configuration[Common.FRONTEND_ORIGIN] ?? "http://localhost:3000";
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithOrigins(frontEndOrigin);
                });
            });

            //add repo
            builder.Services.AddScoped<IStoreIngredientRepository, StoreIngredientRepository>();
            builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
            builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();

            //add client
            builder.Services.AddScoped<IIdentityServiceClient, IdentityServiceClient>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sale Service API v1");
            //    });
            //}

            app.UseCors();

            // Add global exception handler middleware
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<InventoryGrpcService>();
            app.MapGrpcService<InventoryInternalService>();
            app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
            app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");

            app.Run();
        }
    }
}