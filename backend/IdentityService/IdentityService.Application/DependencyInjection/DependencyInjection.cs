using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Application.Behaviors;

namespace IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
