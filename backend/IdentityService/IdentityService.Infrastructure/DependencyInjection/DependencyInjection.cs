using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.ExternalServices;
using IdentityService.Infrastructure.Interceptor;
using IdentityService.Infrastructure.Persistences.Cache;
using IdentityService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace IdentityService.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string identityDbConn, string redisConnectionString)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddDbContext<IIdentityDbContext, IdentityDbContext>(opt => opt.UseNpgsql(identityDbConn));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
            configuration.AbortOnConnectFail = false;
            configuration.DefaultDatabase = 0;
            return ConnectionMultiplexer.Connect(configuration);
        });
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConnectionString + ",defaultDatabase=1";
            opt.InstanceName = "chb_redis";
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITokenBlacklist, TokenBlacklist>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpStore, OtpStore>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddSingleton<IPermissionCache, PermissionCache>();

        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}