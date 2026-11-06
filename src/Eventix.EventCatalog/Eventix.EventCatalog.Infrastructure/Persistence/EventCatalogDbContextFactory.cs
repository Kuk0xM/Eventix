using Eventix.EventCatalog.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Eventix.EventCatalog.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating the DbContext when running EF Core tools
/// (migrations, updates, etc.) outside of the running application.
/// This prevents EF Core from having to execute the full Program.cs.
/// </summary>
public class EventCatalogDbContextFactory : IDesignTimeDbContextFactory<EventCatalogDbContext>
{
    public EventCatalogDbContext CreateDbContext(string[] args)
    {
        // 1. Build configuration from appsettings and environment variables
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // 2. Get connection string (can come from args, env var, or appsettings)
        var connectionString = GetConnectionString(args, configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Pass it via --connection argument or " +
                "set ConnectionStrings__EventCatalogDatabase environment variable.");
        }

        // 3. Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<EventCatalogDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // Note: We use a null interceptor here because in design-time
        // we don't need domain event publishing
        return new EventCatalogDbContext(
            optionsBuilder.Options,
            new PublishDomainEventsInterceptor());
    }

    private static string? GetConnectionString(string[] args, IConfiguration configuration)
    {
        // Priority 1: Connection string passed via --connection argument
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            return args[0];
        }

        // Priority 2: Environment variable (GitHub Actions sets this)
        var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__EventCatalogDatabase");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            return envConnectionString;
        }

        // Priority 3: appsettings.json (for local development)
        return configuration.GetConnectionString("EventCatalogDatabase");
    }
}