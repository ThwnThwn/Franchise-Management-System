using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Behaviors;
using ProductService.Application.Mapping;

namespace ProductService.Application.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Configure AutoMapper
        // Auto scan all Profile classes from API and Application assemblies
        var apiAssembly = Assembly.GetExecutingAssembly();
        var applicationAssembly = typeof(ProductMappingProfile).Assembly;

        services.AddSingleton(_ =>
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(apiAssembly);           // Scan API assembly (ProductGrpcMappingProfile)
                c.AddMaps(applicationAssembly);    // Scan Application assembly (ProductMappingProfile)
            });
            return cfg.CreateMapper();
        });

        // Register MediatR
        services.AddMediatR(typeof(ValidationBehavior<,>).Assembly);

        // Register FluentValidation
        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

        // Register ValidationBehavior for MediatR Pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}