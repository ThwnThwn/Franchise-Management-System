using FluentValidation;
using InventoryService.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(DependencyInjection).Assembly);
        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
