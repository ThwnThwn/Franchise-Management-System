using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FranchiseService.Application.Repositories;
using FranchiseService.Infrastructure.Repositories;

namespace FranchiseService.Infrastructure.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string identityDbConn)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddDbContext<IFranchiseDbContext, FranchiseDbContext>(opt => opt.UseNpgsql(identityDbConn));

        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();

        return services;
    }
}