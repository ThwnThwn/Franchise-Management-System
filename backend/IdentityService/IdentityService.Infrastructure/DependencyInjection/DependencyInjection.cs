using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Application.ExternalServices;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.ExternalServices;
using SharedKernel.Infrastructure.Kafka;
using IdentityService.Infrastructure.Interceptor;
using IdentityService.Infrastructure.Persistences.Cache;
using IdentityService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace IdentityService.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    /// <summary>
    /// Adds the infrastructure.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="identityDbConn">The identity database connection.</param>
    /// <param name="redisConnectionString">The redis connection string.</param>
    /// <param name="blackListDb">The black list database.</param>
    /// <param name="permDb">The perm database.</param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string identityDbConn,
        string redisConnectionString, string blackListDb, string permDb)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddDbContext<IIdentityDbContext, IdentityDbContext>(opt => opt.UseNpgsql(identityDbConn));

        if (!int.TryParse(blackListDb, out var blackListDbInt))
        {
            blackListDbInt = 0;
        }
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
            configuration.AbortOnConnectFail = false;
            configuration.DefaultDatabase = blackListDbInt;
            return ConnectionMultiplexer.Connect(configuration);
        });
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisConnectionString + ",defaultDatabase=" + permDb;
            opt.InstanceName = "chb_redis";
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITokenBlacklist, TokenBlacklist>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpStore, OtpStore>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddSingleton<IPermissionCache, PermissionCache>();

        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}