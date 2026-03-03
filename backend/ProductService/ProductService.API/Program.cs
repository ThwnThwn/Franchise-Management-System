using DotNetEnv;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProductService.API.Middleware;
using ProductService.API.Services;
using ProductService.Application.Extentions;
using ProductService.Infrastructure.Extensions;

namespace ProductService.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Load .env file
        Env.Load("../../../.env");

        var builder = WebApplication.CreateBuilder(args);

        // Add environment variables to configuration
        builder.Configuration.AddEnvironmentVariables();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            });
        });

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection(); // Enable gRPC Server Reflection
        builder.Services.AddControllers();

        builder.Services.AddInfrastructures(builder.Configuration);
        builder.Services.AddApplication();

        // Register GlobalExceptionHandlerMiddleware
        builder.Services.AddSingleton<GlobalExceptionHandlerMiddleware>();

        // Register gRPC Exception Interceptor
        builder.Services.AddSingleton<Interceptors.GrpcExceptionInterceptor>();

        var app = builder.Build();

        app.MapGrpcService<ProductGrpcService>();
        app.MapGrpcService<CategoryGrpcService>();
        app.MapGrpcService<ProductVariantInternalGrpcService>();
        app.MapGrpcReflectionService(); // Enable gRPC Server Reflection endpoint

        app.Run();
    }
}