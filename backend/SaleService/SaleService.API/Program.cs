using System.Text;
using AutoMapper;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SalesService.API.GrpcServices;
using SalesService.API.Middleware;
using SalesService.API.Services.CartItems;
using SalesService.API.Services.Identity;
using SalesService.API.Services.Inventory;
using SalesService.API.Services.Orders;
using SalesService.API.Services.Payments;
using SalesService.API.Services.Products;
using SalesService.API.Services.Wallets;
using SalesService.Application.Authorization;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
//using SalesService.Infrastructure.Models;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.OrderItems;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.WalletEvents;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Signals;
using SalesService.Grpc;
using SalesService.Infrastructure.ApplicationDbContext;
using SalesService.Infrastructure.Extensions;
using SalesService.Infrastructure.Interceptor;
using SalesService.Infrastructure.Repositories;
using SalesService.Infrastructure.Repositories;
using SalesService.Infrastructure.Repositories.CartItems;
using SalesService.Infrastructure.Repositories.OrderItems;
using SalesService.Infrastructure.Repositories.Orders;
using SalesService.Infrastructure.Repositories.Outbox;
using SalesService.Infrastructure.Repositories.Payments;
using SalesService.Infrastructure.Repositories.WalletEvents;
using SalesService.Infrastructure.Signals;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
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
        //var saleServicePort = int.Parse(builder.Configuration["SALES_PORT"] ?? "5003");
        //builder.WebHost.ConfigureKestrel(options =>
        //{
        //    options.ListenLocalhost(saleServicePort, listenOptions =>
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
        builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

        builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
        builder.Services.AddScoped<IWalletRepository, WalletRepository>();
        builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        builder.Services.AddScoped<IRefundTicketRepository, RefundTicketRepository>();
        builder.Services.AddScoped<IRefundAuthorizationService, RefundAuthorizationService>();
        builder.Services.AddScoped<IWalletAuthorizationService, WalletAuthorizationService>();
        builder.Services.AddScoped<IWalletEventRepository, WalletEventRepository>();
        // Register ProductService gRPC client
        builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();

        // Register IdentityService gRPC client
        builder.Services.AddSingleton<IIdentityServiceClient, IdentityServiceClient>();

        // Register Kafka Producer Registry
        builder.Services.AddSingleton<KafkaProducerRegistry>(sp =>
        {
            var registry = new KafkaProducerRegistry();
            registry.Register<OrderStatusEvent>(KafkaTopics.OrderNotifications, KafkaTopics.OrderNotifications);
            registry.Register<OrderPaidEvent>(KafkaTopics.OrderPaid, KafkaTopics.OrderPaid);
            registry.Register<OrderCancelledEvent>(KafkaTopics.OrderCancelled, KafkaTopics.OrderCancelled);
            return registry;
        });

        // Register Kafka Producer
        builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

        // Register GlobalExceptionHandlerMiddleware
        builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

        // Register gRPC Exception Interceptor
        builder.Services.AddSingleton<Interceptors.GrpcExceptionInterceptor>();

        //Inventory Service Client
        builder.Services.AddScoped<IInventoryServiceClient, InventoryServiceClient>();

        //Wallet 
        builder.Services.AddScoped<IRefundTicketRepository, RefundTicketRepository>();
        builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        // Signal for Outbox
        builder.Services.AddSingleton<IOutboxSignal, OutboxSignal>();

        // Register Background Jobs
        builder.Services.AddHostedService<SalesService.API.BackgroundJobs.OutboxWorker>();

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

        builder.Services.AddSwaggerGen(options =>
        {
            // 🔥 FIX duplicate DTO schema issue
            options.CustomSchemaIds(type => type.FullName);

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Sale Service API",
                Version = "v1",
                Description = "API quản lý đơn hàng và bán hàng"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhập token theo format: Bearer {your token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
        });


        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["JWT_SECRET"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "CFSM-be",
            ValidAudience = "CFSM-be",

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret)
            ),

           RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsync(
                    $"Authentication failed: {context.Exception.Message}");
            },

            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsync(
                    $"Unauthorized: {context.ErrorDescription}");
            },

            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                return context.Response.WriteAsync(
                    "Forbidden: You do not have required role.");
            }
        };
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGrpcService<OrderGrpcService>();
        app.MapGrpcService<PaymentGrpcService>();
        app.MapGrpcService<CartItemGrpcService>();
        app.MapGrpcService<WalletGrpcService>();
        app.MapGrpcService<WalletTransactionGrpcService>();
        app.MapGrpcService<RefundGrpcService>();
        app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
        app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");


        app.Run();
    }
}