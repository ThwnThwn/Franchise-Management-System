using AutoMapper;
using DotNetEnv;
using InventoryService.API.Middleware;
using InventoryService.API.Services.Identity;
using InventoryService.Application.Constants;
using InventoryService.Application.DependencyInjection;
using InventoryService.Infrastructure.ApplicationDbContext; // Added back for concrete class
using InventoryService.Infrastructure.Interceptor;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Infrastructure.Persistences.Repositories;

//using InventoryService.Application.ExternalServices;
//using InventoryService.Infrastructure.ExternalServices;
using InventoryService.Application.Signals;
using SharedKernel.Infrastructure.Kafka;
using InventoryService.Application.Signals;
using InventoryService.Infrastructure.Signals;

using InventoryService.API.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using InventoryService.API.Services.Inventory;
using InventoryService.Application.Abstractions;
using InventoryService.API.Services.IngredientCategory;
using InventoryService.API.Services.Recipe;
using InventoryService.Application.ClientInterface;
using InventoryService.API.Services.Product;

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

            // KAFKA - Override for local development
            var kafkaBootstrap = builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
            if (kafkaBootstrap.Contains("kafka:"))
            {
                kafkaBootstrap = "localhost:9092";  // Use localhost when running outside Docker
            }
            builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] = kafkaBootstrap;

            // Configure Kestrel for gRPC (HTTPS + HTTP/2)
            var inventoryServicePort = int.Parse(builder.Configuration[Common.INVENTORY_PORT] ?? "5005");
            //var inventoryServicePort = int.Parse(builder.Configuration["INVENTORY_PORT"] ?? "5005");
            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ListenLocalhost(inventoryServicePort, listenOptions =>
            //    {
            //        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            //        listenOptions.UseHttps(); // Development certificate
            //    });
            //});
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    // Allow both HTTP/1.1 (REST) and HTTP/2 (gRPC)
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
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

            // Interceptors
            builder.Services.AddScoped<UpdateTimestampInterceptor>();

            // Connect Db
            var connectionString = builder.Configuration[Common.INVENTORY_DB_CONNECTION];

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
            //builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

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
            builder.Services.AddScoped<IIngredientCategoryRepository, IngredientCategoryRepository>();
            builder.Services.AddScoped<InventoryService.Domain.Services.IInventoryDomainService, InventoryService.Domain.Services.InventoryDomainService>();
            //add client
            builder.Services.AddScoped<IIdentityServiceClient, IdentityServiceClient>();
            builder.Services.AddScoped<IProductServiceClient, ProductServiceClient>();
            // Kafka Producer Registry (for Outbox)
            builder.Services.AddSingleton<KafkaProducerRegistry>(sp => {
                var registry = new KafkaProducerRegistry();
                // Map "LowStockDetected" (stored in DB) -> LowStockEvent -> KafkaTopics.LowStockNotifications
                registry.Register<SharedKernel.Events.LowStockEvent>(SharedKernel.Constants.KafkaTopics.LowStockNotifications, "LowStockDetected");
                return registry;
            });

            // Kafka Producer
            builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

            // Outbox Pattern
            builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
            builder.Services.AddSingleton<IOutboxSignal, OutboxSignal>();
            builder.Services.AddHostedService<API.BackgroundJobs.InventoryOutboxWorker>();
            // Kafka Consumer (Generic)
            builder.Services.AddKafkaConsumer(config =>
            {
                config.Register<SharedKernel.Events.OrderPaidEvent>(SharedKernel.Constants.KafkaTopics.OrderPaid, typeof(InventoryService.Application.EventHandlers.OrderPaidHandler));
                config.Register<SharedKernel.Events.OrderCancelledEvent>(SharedKernel.Constants.KafkaTopics.OrderCancelled, typeof(InventoryService.Application.EventHandlers.OrderCancelledHandler));
            });

            // builder.Services.AddHostedService<InventoryService.API.Consumers.OrderEventConsumer>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
            //app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<InventoryGrpcService>();
            app.MapGrpcService<InventoryInternalService>();
            app.MapGrpcService<IngredientGrpcService>();
            app.MapGrpcService<CategoryGrpcService>();
            app.MapGrpcService<RecipeGrpcService>();
            app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
            app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");

            app.Run();
        }
    }
}