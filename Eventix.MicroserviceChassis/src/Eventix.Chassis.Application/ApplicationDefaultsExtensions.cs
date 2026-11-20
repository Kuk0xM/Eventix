using Eventix.Chassis.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.Chassis.Application;

public static class ApplicationDefaultsExtensions
{
    /// <summary>
    /// Registers MediatR + all standard pipeline behaviors (Logging, Validation, Performance).
    /// Call this from your Application layer's DependencyInjection.
    /// </summary>
    public static IServiceCollection AddEventixApplication(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}