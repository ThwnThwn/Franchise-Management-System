using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FranchiseService.Application.Interfaces;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Settings;
using FranchiseService.Application.Signals;
using FranchiseService.Infrastructure.Repositories;
using FranchiseService.Infrastructure.Services;
using FranchiseService.Infrastructure.Settings;
using FranchiseService.Infrastructure.Signals;
using SharedKernel.Infrastructure.Kafka;

namespace FranchiseService.Infrastructure.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string identityDbConn, IConfiguration config)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddDbContext<IFranchiseDbContext, FranchiseDbContext>(opt => opt.UseNpgsql(identityDbConn));

        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IFranchiseApplicationRepository, FranchiseApplicationRepository>();
        services.AddScoped<IFranchiseContractRepository, FranchiseContractRepository>();
        services.AddScoped<IFranchiseUow, FranchiseUow>();

        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Cloudinary (file / image upload) — bind thủ công từ env vars (đồng nhất với các service khác)
        services.Configure<CloudinarySettings>(options =>
        {
            options.CloudName = config["CLOUDINARY_CLOUD_NAME"] ?? string.Empty;
            options.ApiKey = config["CLOUDINARY_API_KEY"] ?? string.Empty;
            options.ApiSecret = config["CLOUDINARY_API_SECRET"] ?? string.Empty;
        });
        services.AddScoped<IImageService, CloudinaryService>();

        // DocuSeal (e-signature) - Load from Env like other services
        services.Configure<DocuSealSettings>(options =>
        {
            options.BaseUrl = config["DOCUSEAL_BASE_URL"] ?? "https://api.docuseal.com";
            options.ApiToken = config["DOCUSEAL_API_TOKEN"] ?? string.Empty;
            options.WebhookSecret = config["DOCUSEAL_WEBHOOK_SECRET"] ?? string.Empty;
            if (int.TryParse(config["DOCUSEAL_TEMPLATE_ID"], out var templateId))
                options.TemplateId = templateId;
        });
        services.AddHttpClient("DocuSeal");
        services.AddScoped<IDocuSealService, DocuSealService>();

        // Franchise contract defaults (revenue share, duration, deposit, franchise fee)
        services.Configure<FranchiseSettings>(options =>
        {
            if (decimal.TryParse(config["FRANCHISE_DEFAULT_REVENUE_SHARE"], out var revenueShare))
                options.DefaultRevenueSharePercentage = revenueShare;
            if (int.TryParse(config["FRANCHISE_DEFAULT_DURATION_YEARS"], out var durationYears))
                options.DefaultDurationYears = durationYears;
            if (decimal.TryParse(config["FRANCHISE_DEFAULT_DEPOSIT_AMOUNT"], out var depositAmount))
                options.DefaultDepositAmount = depositAmount;
            if (decimal.TryParse(config["FRANCHISE_DEFAULT_FRANCHISE_FEE"], out var franchiseFee))
                options.DefaultFranchiseFee = franchiseFee;
        });

        // Geocoding (Lấy Lat / Lng từ Address)
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>();

        // Outbox Pattern
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Signal (Singleton để Shared toàn bộ App)
        services.AddSingleton<IOutboxSignal, OutboxSignal>();

        // Kafka Producer
        services.AddSingleton<KafkaProducerRegistry>();
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        return services;
    }
}