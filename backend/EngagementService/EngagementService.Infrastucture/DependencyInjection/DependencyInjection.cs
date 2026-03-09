using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Infrastucture.Interceptor;
using EngagementService.Infrastucture.Persistences.BackgroundJob;
using EngagementService.Infrastucture.Persistences.Cache;
using EngagementService.Infrastucture.Persistences.Repository;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure.Kafka;
using StackExchange.Redis;

namespace EngagementService.Infrastucture.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string redisConnectionString,
        string tierDb)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConnectionString + ",defaultDatabase=" + tierDb;
            opt.InstanceName = "chb_redis";
        });

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

        services.AddScoped<ILoyaltyAccountUow, LoyaltyAccountUow>();


        services.AddHostedService<CleanupPointLogJob>();

        

        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}