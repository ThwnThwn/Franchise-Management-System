using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Behaviors;

namespace SalesService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(typeof(ValidationBehavior<,>).Assembly);

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

        // Register ValidationBehavior for MediatR Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
