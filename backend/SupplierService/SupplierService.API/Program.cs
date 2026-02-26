using DotNetEnv;
using MediatR;
using InventoryService.Grpc.Protos.Inventory;
using Microsoft.EntityFrameworkCore;
using SupplierService.Application.ClientInterfaces;
using SupplierService.Application.Repositories;
using SupplierService.Infrastructure.DbContext;
using SupplierService.API.Clients.Inventory;
using SupplierService.Infrastructure.Interceptor;
using SupplierService.Infrastructure.Repositories;

namespace SupplierService.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Load .env file
        Env.Load("../../../.env");

        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        // Configure Kestrel
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            });
        });

        // ── DbContext (supplier_db) ───────────────────────────────────────────
        var connectionString = builder.Configuration["SUPPLIER_DB_CONNECTION"];
        builder.Services.AddScoped<UpdateTimestampInterceptor>();
        builder.Services.AddDbContext<SupplierDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(connectionString);
            opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
        });
        builder.Services.AddScoped<ISupplierDbContext>(sp => sp.GetRequiredService<SupplierDbContext>());

        // ── gRPC Server ───────────────────────────────────────────────────────
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<SupplierService.API.Interceptors.GrpcExceptionInterceptor>();
        });
        builder.Services.AddGrpcReflection();

        // ── MediatR ───────────────────────────────────────────────────────────
        builder.Services.AddMediatR(
            typeof(SupplierService.Application.Features.PurchaseOrders.Commands.CompletePurchaseOrder.CompletePurchaseOrderCommand).Assembly
        );

        // ── Repositories ──────────────────────────────────────────────────────
        builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();

        // ── gRPC Client → InventoryService ────────────────────────────────────


        builder.Services.AddScoped<IInventoryServiceClient, InventoryServiceClient>();

        // ── Controllers / Swagger ─────────────────────────────────────────────
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Supplier Service API",
                Version = "v1",
                Description = "API quản lý nhà cung cấp và đơn nhập hàng"
            });
        });

        // ── CORS ──────────────────────────────────────────────────────────────
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

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Supplier Service API v1");
            });
        }

        app.UseCors();
        app.UseAuthorization();
        app.MapControllers();

        // ── gRPC endpoints ────────────────────────────────────────────────────
        app.MapGrpcService<SupplierService.API.Services.PurchaseOrders.PurchaseOrderGrpcService>();
        app.MapGrpcReflectionService();

        app.MapGet("/", () => "Supplier Service API is running.");

        app.Run();
    }
}
