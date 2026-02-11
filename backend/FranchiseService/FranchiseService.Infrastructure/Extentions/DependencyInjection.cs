using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FranchiseService.Application.Repositories;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Infrastructure.Repositories;
using FranchiseService.Infrastructure.Persistences.Repositories;
using FranchiseService.Infrastructure.Signals;
using SharedKernel.Infrastructure.Kafka;
using FranchiseService.Application.Signals;

namespace FranchiseService.Infrastructure.Extentions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string identityDbConn)
    {
        services.AddScoped<UpdateTimestampInterceptor>();

        services.AddDbContext<IFranchiseDbContext, FranchiseDbContext>(opt => opt.UseNpgsql(identityDbConn));

        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        
        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
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