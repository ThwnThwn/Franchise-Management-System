using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastructure.Interceptor;
using EngagementService.Infrastructure.Persistences.BackgroundJob;
using EngagementService.Infrastructure.Persistences.Cache;
using EngagementService.Infrastructure.Persistences.Repository;
using IdentityService.Application.Abstrations;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure.Kafka;
using StackExchange.Redis;

namespace EngagementService.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string redisConnectionString,
        string tierDb,
        string campaignDb)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });
        if (!int.TryParse(campaignDb, out var campaignDbInt)) campaignDbInt = 3;

        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConnectionString + ",defaultDatabase=" + tierDb;
            opt.InstanceName = "chb_redis";
        });
        services.AddSingleton<ICampaignCache>(sp =>
            new CampaignCache(sp.GetRequiredService<IConnectionMultiplexer>(), campaignDbInt));

        //Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
        services.AddScoped<IMembershipTierRepository, MembershipTierRepository>();
        services.AddScoped<IMembershipTierCache, MembershipTierCache>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IPointLogRepository, PointLogRepository>();
        services.AddScoped<IPointRuleRepository, PointRuleRepository>();
        services.AddScoped<IPointRuleCache, PointRuleCache>();
        services.AddScoped<IVoucherItemRepository, VoucherItemRepository>();
        services.AddScoped<IVoucherQueueRepository, VoucherQueueRepository>();
        services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();
        services.AddScoped<IVoucherUsageRepository, VoucherUsageRepository>();
        services.AddScoped<ICampaignRepository, CampaginRepository>();
        services.AddScoped<IEngagementUow, EngagementUow>();


        services.AddHostedService<CleanupPointLogJob>();

        

        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}