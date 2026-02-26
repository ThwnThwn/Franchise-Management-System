using DotNetEnv;
using Microsoft.Extensions.Configuration;

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
            .WithHttpsEndpoint(name: "grpc", port: identityPort);

        // Product Service
        var productPort = int.Parse(builder.Configuration["PRODUCT_PORT"] ?? "5002");
        builder.AddProject<Projects.ProductService_API>("product-service")
            .WithHttpsEndpoint(name: "grpc", port: productPort);

        // Sales Service
        var salesPort = int.Parse(builder.Configuration["SALES_PORT"] ?? "5003");
        builder.AddProject<Projects.SalesService_API>("sales-service")
            .WithHttpsEndpoint(name: "grpc", port: salesPort);

        // Notification Service
        var notificationPort = int.Parse(builder.Configuration["NOTIFICATION_PORT"] ?? "5004");
        builder.AddProject<Projects.NotificationService_API>("notification-service")
            .WithHttpsEndpoint(name: "grpc", port: notificationPort);

        // Inventory Service
        var inventoryPort = int.Parse(builder.Configuration["INVENTORY_PORT"] ?? "5005");
        builder.AddProject<Projects.InventoryService_API>("inventory-service")
            .WithHttpsEndpoint(name: "grpc", port: inventoryPort);

        // Franchise Service
        var franchisePort = int.Parse(builder.Configuration["FRANCHISE_PORT"] ?? "5006");
        builder.AddProject<Projects.FranchiseService_API>("franchise-service")
            .WithHttpsEndpoint(name: "grpc", port: franchisePort);

        // Engagement Service
        var engagementPort = int.Parse(builder.Configuration["ENGAGEMENT_PORT"] ?? "5007");
        builder.AddProject<Projects.EngagementService_API>("engagement-service")
            .WithHttpsEndpoint(name: "grpc", port: engagementPort);
        // Supplier Service
        var supplierPort = int.Parse(builder.Configuration["SUPPLIER_PORT"] ?? "5008");
        builder.AddProject<Projects.SupplierService_API>("supplier-service")
            .WithHttpsEndpoint(name: "grpc", port: supplierPort);

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
        //       .WithHttpsEndpoint(name: "api", port: gatewayPort, targetPort: gatewayPort, isProxied: false);

        builder.Build().Run();
    }
}