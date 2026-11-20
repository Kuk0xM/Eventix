using Eventix.Chassis.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Eventix.EventCatalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR + Logging/Validation/Performance pipeline behaviors
        services.AddEventixApplication(typeof(DependencyInjection).Assembly);

        return services;
    }
}