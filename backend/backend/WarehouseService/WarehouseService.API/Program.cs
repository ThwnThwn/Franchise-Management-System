using DotNetEnv;
using FranchiseService.Grpc;
using InventoryService.Grpc.Protos.Ingredient;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using SupplierService.Grpc.Protos.Supplier;
using WarehouseService.API.Clients;
using WarehouseService.API.Interceptors;
using WarehouseService.API.Middleware;
using WarehouseService.API.Services;
using WarehouseService.Application.ClientInterfaces;
using WarehouseService.Application.Extensions;
using WarehouseService.Application.Repositories;
using WarehouseService.Infrastructure.DbContext;
using WarehouseService.Infrastructure.Interceptor;
using WarehouseService.Infrastructure.Repositories;

namespace WarehouseService.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Load .env file
        Env.Load("../../../.env");

        var builder = WebApplication.CreateBuilder(args);

        // Add environment variables to configuration
        builder.Configuration.AddEnvironmentVariables();

        // ── Kestrel: HTTP/2 only ──────────────────────────────────────────────
        var warehousePort = int.Parse(builder.Configuration["WAREHOUSE_PORT"] ?? "5009");
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(warehousePort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });

        // ── gRPC (chỉ đăng ký 1 lần) ─────────────────────────────────────────
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });
        builder.Services.AddGrpcReflection();

        // ── Interceptors ──────────────────────────────────────────────────────
        builder.Services.AddScoped<UpdateTimestampInterceptor>();
        builder.Services.AddSingleton<GrpcExceptionInterceptor>();

        // ── Database ──────────────────────────────────────────────────────────
        var connectionString = builder.Configuration["WAREHOUSE_DB_CONNECTION"];
        builder.Services.AddDbContext<WarehouseDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(connectionString);
            opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
        });
        builder.Services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());

        // ── Application (MediatR + FluentValidation) ──────────────────────────
        builder.Services.AddApplication();

        // ── Repositories ──────────────────────────────────────────────────────
        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        builder.Services.AddScoped<IWarehouseItemRepository, WarehouseItemRepository>();
        builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        builder.Services.AddScoped<IWarehouseTransferRepository, WarehouseTransferRepository>();
        builder.Services.AddScoped<IWarehouseTransferItemRepository, WarehouseTransferItemRepository>();


        // ── gRPC Clients ──────────────────────────────────────────────────────
        builder.Services.AddSingleton<IInventoryServiceClient, InventoryServiceClient>();
        builder.Services.AddSingleton<ISupplierServiceClient, SupplierServiceClient>();
        builder.Services.AddSingleton<IFranchiseServiceClient, FranchiseServiceClient>();
        builder.Services.AddGrpcClient<SupplierGrpc.SupplierGrpcClient>(o =>
        {
            var url = builder.Configuration["SUPPLIER_SERVICE_URL"];

            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("Missing configuration: SUPPLIER_SERVICE_URL");
            }

            o.Address = new Uri(url);
        });
        // 1. Cấu hình cho Ingredient Service
        builder.Services.AddGrpcClient<IngredientGrpc.IngredientGrpcClient>(o =>
        {
            var ingredientUrl = builder.Configuration["INVENTORY_SERVICE_URL"];
            if (string.IsNullOrEmpty(ingredientUrl))
            {
                // Nếu không tìm thấy trong ENV/Appsettings, báo lỗi ngay khi khởi động
                throw new InvalidOperationException("Missing configuration: INVENTORY_SERVICE_URL");
            }
            o.Address = new Uri(ingredientUrl);
        });

        // 2. Cấu hình cho Franchise (Store) Service
        builder.Services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(o =>
        {
            var franchiseUrl = builder.Configuration["FRANCHISE_SERVICE_URL"];
            if (string.IsNullOrEmpty(franchiseUrl))
            {
                throw new InvalidOperationException("Missing configuration: FRANCHISE_SERVICE_URL");
            }
            o.Address = new Uri(franchiseUrl);
        });

        // ── Middleware ────────────────────────────────────────────────────────
        builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

        var app = builder.Build();

        // ── Middleware pipeline ───────────────────────────────────────────────
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // ── gRPC endpoints ────────────────────────────────────────────────────
        app.MapGrpcService<WarehouseGrpcService>();
        app.MapGrpcService<WarehouseItemGrpcService>();
        app.MapGrpcService<StockTransactionGrpcService>();
        app.MapGrpcService<WarehouseTransferGrpcService>();
        app.MapGrpcReflectionService();

        app.MapGet("/", () => "WarehouseService gRPC is running on port 5009 (HTTP/2 only).");

        app.Run();
    }
}