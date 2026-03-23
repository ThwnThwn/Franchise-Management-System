using System.Text;
using AutoMapper;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SalesService.API.GrpcServices;
using SalesService.API.Middleware;
using SalesService.API.Services.CartItems;
using SalesService.API.Services.Identity;
using SalesService.API.Services.Franchise;
using SalesService.API.Services.Inventory;
using SalesService.API.Services.Orders;
using SalesService.API.Services.Payments;
using SalesService.API.Services.Products;
using SalesService.API.Services.Wallets;
using SalesService.API.Services.Withdrawal;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.API.Services.Geocoding;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.Geocoding;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Interfaces.Identity;
using SalesService.Application.Mapping.Orders;
using SalesService.Application.Signals;
using SalesService.Infrastructure.Extensions;
using SalesService.Infrastructure.ExternalServices;
using SalesService.Infrastructure.Signals;
using SharedKernel.Constants;
using SharedKernel.Events;
using SharedKernel.Infrastructure.Kafka;
using Yarp.ReverseProxy.Configuration;
using SalesService.API.Services.Distance;

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

        //builder.WebHost.ConfigureKestrel(options =>
        //{
        //    options.ConfigureEndpointDefaults(listenOptions =>
        //    {
        //        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        //    });
        //});
        var salesPort = int.Parse(builder.Configuration["SALES_PORT"] ?? "5003");
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(salesPort, listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });

            options.ListenAnyIP(5013, listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
            });
        });

        // Add services to the container.
        var maxUploadSizeMB = int.Parse(builder.Configuration["MAX_UPLOAD_SIZE_MB"] ?? "10");
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<Interceptors.GrpcExceptionInterceptor>();

            options.MaxReceiveMessageSize = maxUploadSizeMB * 1024 * 1024;
            options.MaxSendMessageSize = maxUploadSizeMB * 1024 * 1024;
        });
        builder.Services.AddGrpcReflection(); // Enable gRPC Server Reflection
        builder.Services.AddControllers();

        // ===== YARP REVERSE PROXY (Dev: Forward SupplierService VNPay callbacks) =====
        // Ngrok chỉ có 1 domain trỏ vào SaleService:5013
        // Các request có path /api/supplier/vnpay/* sẽ được forward sang SupplierService
        var supplierServiceUrl = builder.Configuration["SUPPLIER_SERVICE_URL"] ?? "http://localhost:5008";
        builder.Services.AddReverseProxy()
            .LoadFromMemory(
                routes: new[]
                {
                    new RouteConfig
                    {
                        RouteId    = "supplier_vnpay_route",
                        ClusterId  = "supplier_cluster",
                        Match      = new RouteMatch { Path = "/api/supplier/vnpay/{**catch-all}" },
                        Transforms = new[]
                        {
                            // /api/supplier/vnpay/{catch-all} → /api/vnpay/{catch-all}
                            // PathRemovePrefix bị thiếu /api nên dùng PathPattern thay thế
                            new Dictionary<string, string> { ["PathPattern"] = "/api/vnpay/{**catch-all}" }
                        }
                    }
                },
                clusters: new[]
                {
                    new ClusterConfig
                    {
                        ClusterId    = "supplier_cluster",
                        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["supplier_dest"] = new DestinationConfig { Address = supplierServiceUrl }
                        }
                    }
                }
            );

        // Add API documentation
        builder.Services.AddEndpointsApiExplorer();

        // KAFKA - Override for local development
        var kafkaBootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") 
                     ?? builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] 
                     ?? "localhost:9092";

        bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        if (!isDocker)
        {
            kafkaBootstrap = "localhost:9092";
        }
        builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] = kafkaBootstrap;
        
        // KAFKA CONSUMER (Generic)
        builder.Services.AddKafkaConsumer(config =>
        {
            config.Register<UserRegisteredEvent>(KafkaTopics.WalletAccount, typeof(SalesService.Application.EventHandler.CreateWalletEvent.CreateWalletEventConsumer));
        });
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
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // Register ProductService gRPC client
        builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();

        // Register IdentityService gRPC client
        builder.Services.AddSingleton<IIdentityServiceClient, IdentityServiceClient>();

        // Register FranchiseService gRPC client
        builder.Services.AddSingleton<SalesService.Application.Interfaces.Franchise.IFranchiseServiceClient, FranchiseServiceClient>();

        // Register DistanceService
        builder.Services.AddHttpClient<SalesService.Application.Interfaces.IDistanceService, DistanceService>();

        // Register GeocodingService
        builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();

        // Register Kafka Producer Registry
        builder.Services.AddSingleton<KafkaProducerRegistry>(sp =>
        {
            var registry = new KafkaProducerRegistry();
            registry.Register<OrderStatusEvent>(KafkaTopics.OrderNotifications, KafkaTopics.OrderNotifications);
            registry.Register<OrderPaidEvent>(KafkaTopics.OrderPaid, KafkaTopics.OrderPaid);
            registry.Register<OrderCancelledEvent>(KafkaTopics.OrderCancelled, KafkaTopics.OrderCancelled);
            registry.Register<ProcessOrderRewardEvent>(KafkaTopics.ProcessOrderReward, KafkaTopics.ProcessOrderReward);
            registry.Register<UseVouchersEvent>(KafkaTopics.DeductUserVoucher, KafkaTopics.DeductUserVoucher);
            registry.Register<UpdatePointEvent>(KafkaTopics.UpdateUserPoint, KafkaTopics.UpdateUserPoint);
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

        // Engagement Serivce Client
        builder.Services.AddScoped<IEngagementServiceClient, EngagementSeviceClient>();

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

        // Map YARP TRƯỚC controllers để /api/supplier/vnpay/* được bắt trước
        app.MapReverseProxy();
        app.MapControllers();
        app.MapGrpcService<OrderGrpcService>();
        app.MapGrpcService<PaymentGrpcService>();
        app.MapGrpcService<CartItemGrpcService>();
        app.MapGrpcService<WalletGrpcService>();
        app.MapGrpcService<WalletTransactionGrpcService>();
        app.MapGrpcService<RefundGrpcService>();
        app.MapGrpcService<WithdrawalGrpcService>();
        app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint
        app.MapGet("/", () => "Sale Service API is running. Visit /swagger for API documentation.");

        app.MapGet("/api/test-geocode", async (string address, IGeocodingService geocodingService) =>
        {
            var result = await geocodingService.GeocodeAsync(address);
            return result != null ? Results.Ok(result) : Results.NotFound(new { message = "Address not found", address });
        });
        app.Run();
    }
}