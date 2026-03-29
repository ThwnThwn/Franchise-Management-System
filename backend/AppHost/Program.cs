using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace AppHost;

internal class Program
{
    private static void Main(string[] args)
    {
        Env.Load("../../.env");

        var builder = DistributedApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        // --- 1. KHAI BÁO CÁC SERVICE .NET (Backend) ---

        // Identity Service
        var identityPort = int.Parse(builder.Configuration["IDENTITY_PORT"] ?? "5001");
        builder.AddProject<Projects.IdentityService_API>("identity-service")
            .WithHttpEndpoint(name: "grpc", port: identityPort, isProxied: false);

        // Product Service
        var productPort = int.Parse(builder.Configuration["PRODUCT_PORT"] ?? "5002");
        builder.AddProject<Projects.ProductService_API>("product-service")
            .WithHttpEndpoint(name: "grpc", port: productPort, isProxied: false);

        // Sales Service
        var salesPort = int.Parse(builder.Configuration["SALES_PORT"] ?? "5003");
        builder.AddProject<Projects.SalesService_API>("sales-service")
            .WithHttpEndpoint(name: "grpc", port: salesPort, isProxied: false);

        // Notification Service
        var notificationPort = int.Parse(builder.Configuration["NOTIFICATION_PORT"] ?? "5004");
        builder.AddProject<Projects.NotificationService_API>("notification-service")
            .WithHttpEndpoint(name: "grpc", port: notificationPort, isProxied: false);

        // Inventory Service
        var inventoryPort = int.Parse(builder.Configuration["INVENTORY_PORT"] ?? "5005");
        builder.AddProject<Projects.InventoryService_API>("inventory-service")
            .WithHttpEndpoint(name: "grpc", port: inventoryPort, isProxied: false);

        // Franchise Service
        var franchisePort = int.Parse(builder.Configuration["FRANCHISE_PORT"] ?? "5006");
        builder.AddProject<Projects.FranchiseService_API>("franchise-service")
            .WithHttpEndpoint(name: "grpc", port: franchisePort, isProxied: false);

        // Engagement Service
        var engagementPort = int.Parse(builder.Configuration["ENGAGEMENT_PORT"] ?? "5007");
        builder.AddProject<Projects.EngagementService_API>("engagement-service")
            .WithHttpEndpoint(name: "grpc", port: engagementPort, isProxied: false);

        // Supplier Service
        var supplierPort = int.Parse(builder.Configuration["SUPPLIER_PORT"] ?? "5008");
        builder.AddProject<Projects.SupplierService_API>("supplier-service")
            .WithHttpEndpoint(name: "grpc", port: supplierPort, isProxied: false);

        // Warehouse Service
        var warehousePort = int.Parse(builder.Configuration["WAREHOUSE_PORT"] ?? "5009");
        builder.AddProject<Projects.WarehouseService_API>("warehouse-service")
            .WithHttpEndpoint(name: "grpc", port: warehousePort, isProxied: false);

        // --- 2. KHAI BÁO GO (API Gateway) ---
        //var goWorkingDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../api-gateway"));
        //var goExecutablePath = Path.Combine(goWorkingDir, "bin", "main.exe");

        //if (!File.Exists(goExecutablePath))
        //{
        //    throw new FileNotFoundException($"KHÔNG TÌM THẤY FILE GO: {goExecutablePath}.");
        //}

        //var gatewayPort = int.Parse(builder.Configuration["GATEWAY_PORT"] ?? "8080");

        //builder.AddExecutable("api-gateway", goExecutablePath, goWorkingDir)
        //       .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        //       .WithHttpEndpoint(name: "api", port: gatewayPort, targetPort: gatewayPort, isProxied: false);

        builder.Build().Run();
    }
}