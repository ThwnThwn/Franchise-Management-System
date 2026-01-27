using Application.Persistences.Caches;
using IdentityService.Application.ExternalServices;
using IdentityService.Application.Persistences.Caches;
using IdentityService.Application.Persistences.Repositories;
using IdentityService.Infrastructure.ApplicationDbContext;
using IdentityService.Infrastructure.ExternalServices;
using IdentityService.Infrastructure.Persistences.Cache;
using IdentityService.Infrastructure.Persistences.Repositories;
using Infrastructure.Persistences.Caches;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string identityDbConn)
    {

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITokenBlacklist, TokenBlacklist>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpStore, OtpStore>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}