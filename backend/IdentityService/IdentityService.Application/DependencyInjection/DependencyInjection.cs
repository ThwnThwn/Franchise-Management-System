using FluentValidation;
using IdentityService.Application.Abstrations;
using IdentityService.Application.Behaviors;
using IdentityService.Application.ExternalServices;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPermissionSyncService, PermissionSyncService>();

        services.AddMediatR(typeof(DependencyInjection).Assembly);
        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}