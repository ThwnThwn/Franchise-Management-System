using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Authorization;
using SalesService.Application.Behaviors;
using SalesService.Application.Interfaces;
using SalesService.Application.Interfaces.CartItems;
using SalesService.Application.Interfaces.Payments;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.OrderItems;
using SalesService.Application.Persisitences.Orders;
using SalesService.Application.Persisitences.Outbox;
using SalesService.Application.Persisitences.WalletEvents;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Application.Persisitences.WithdrawalRequest;
using SalesService.Infrastructure.ApplicationDbContext;
using SalesService.Infrastructure.Interceptor;
using SalesService.Infrastructure.Repositories;
using SalesService.Infrastructure.Repositories.CartItems;
using SalesService.Infrastructure.Repositories.OrderItems;
using SalesService.Infrastructure.Repositories.Orders;
using SalesService.Infrastructure.Repositories.Outbox;
using SalesService.Infrastructure.Repositories.Payments;
using SalesService.Infrastructure.Repositories.WalletEvents;
using SalesService.Infrastructure.Repositories.WithdrawalRequests;
using SalesService.Infrastructure.Security;
using SalesService.Infrastructure.Services;
using SalesService.Infrastructure.Settings;

namespace SalesService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(typeof(ValidationBehavior<,>).Assembly);

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

        // Register ValidationBehavior for MediatR Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Connect Db
        var connectionString = configuration["SALES_DB_CONNECTION"];

        services.Configure<CloudinarySettings>(options =>
         {
             options.CloudName = configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty;
             options.ApiKey = configuration["CLOUDINARY_API_KEY"] ?? string.Empty;
             options.ApiSecret = configuration["CLOUDINARY_API_SECRET"] ?? string.Empty;
         });

        // Interceptors
        services.AddScoped<UpdateTimestampInterceptor>();

        // Persistence & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // DbContext + wire interceptor via DI
        services.AddDbContext<SalesDbContext>(
            (sp, opt) =>
            {
                opt.UseNpgsql(connectionString);
                opt.AddInterceptors(sp.GetRequiredService<UpdateTimestampInterceptor>());
            });

        // Expose DbContext via abstraction used by repositories
        services.AddScoped<ISalesDbContext>(sp => sp.GetRequiredService<SalesDbContext>());

        // Repositories (Gom hết vào đây)
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        services.AddScoped<IRefundTicketRepository, RefundTicketRepository>();
        services.AddScoped<IWalletEventRepository, WalletEventRepository>();
        services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();

        // Services
        services.AddScoped<IAesEncryptionService, AesEncryptionService>();
        services.AddScoped<IImageService, CloudinaryService>();
        services.AddScoped<IRefundAuthorizationService, RefundAuthorizationService>();
        services.AddScoped<IWalletAuthorizationService, WalletAuthorizationService>();

        return services;
    }
}