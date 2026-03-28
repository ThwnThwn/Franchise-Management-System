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

// S1118 fix: add static keyword — Program has no instance members
public static class Program
{
    public static void Main(string[] args)
    {
        Env.Load("../../../.env");

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        var warehousePort = int.Parse(builder.Configuration["WAREHOUSE_PORT"] ?? "5009");
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(warehousePort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });

        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });
        builder.Services.AddGrpcReflection();

        builder.Services.AddScoped<UpdateTimestampInterceptor>();
        builder.Services.AddSingleton<GrpcExceptionInterceptor>();

        var connectionString = builder.Configuration["WAREHOUSE_DB_CONNECTION"];
        builder.Services.AddDbContext<WarehouseDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(connectionString);
            opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
        });
        builder.Services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());

        builder.Services.AddApplication();

        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        builder.Services.AddScoped<IWarehouseItemRepository, WarehouseItemRepository>();
        builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        builder.Services.AddScoped<IWarehouseTransferRepository, WarehouseTransferRepository>();
        builder.Services.AddScoped<IWarehouseTransferItemRepository, WarehouseTransferItemRepository>();

        builder.Services.AddSingleton<IInventoryServiceClient, InventoryServiceClient>();
        builder.Services.AddSingleton<ISupplierServiceClient, SupplierServiceClient>();
        builder.Services.AddSingleton<IFranchiseServiceClient, FranchiseServiceClient>();

        builder.Services.AddGrpcClient<SupplierGrpc.SupplierGrpcClient>(o =>
        {
            var url = builder.Configuration["SUPPLIER_SERVICE_URL"];
            if (string.IsNullOrEmpty(url))
                throw new InvalidOperationException("Missing configuration: SUPPLIER_SERVICE_URL");
            o.Address = new Uri(url);
        });

        builder.Services.AddGrpcClient<IngredientGrpc.IngredientGrpcClient>(o =>
        {
            var ingredientUrl = builder.Configuration["INVENTORY_SERVICE_URL"];
            if (string.IsNullOrEmpty(ingredientUrl))
                throw new InvalidOperationException("Missing configuration: INVENTORY_SERVICE_URL");
            o.Address = new Uri(ingredientUrl);
        });

        builder.Services.AddGrpcClient<StoreGrpcService.StoreGrpcServiceClient>(o =>
        {
            var franchiseUrl = builder.Configuration["FRANCHISE_SERVICE_URL"];
            if (string.IsNullOrEmpty(franchiseUrl))
                throw new InvalidOperationException("Missing configuration: FRANCHISE_SERVICE_URL");
            o.Address = new Uri(franchiseUrl);
        });

        builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.MapGrpcService<WarehouseGrpcService>();
        app.MapGrpcService<WarehouseItemGrpcService>();
        app.MapGrpcService<StockTransactionGrpcService>();
        app.MapGrpcService<WarehouseTransferGrpcService>();
        app.MapGrpcReflectionService();

        app.MapGet("/", () => "WarehouseService gRPC is running on port 5009 (HTTP/2 only).");

        app.Run();
    }
}