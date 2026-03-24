using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WarehouseService.Application.Behaviors;

namespace WarehouseService.Application.Extensions;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        // MediatR
        services.AddMediatR(assembly);
        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Validation pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}