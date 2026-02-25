using FluentValidation;
using EngagementService.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using EngagementService.Application.Abstraction;
using EngagementService.Application.ClientInterface;

namespace EngagementService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IMembershipTierSyncService, MembershipTierSyncService>();
        return services;
    }
}
