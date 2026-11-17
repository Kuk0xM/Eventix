using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Eventix.EventCatalog.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating the DbContext when running EF Core tools
/// (migrations, updates, etc.) outside of the running application.
/// </summary>
public class EventCatalogDbContextFactory : IDesignTimeDbContextFactory<EventCatalogDbContext>
{
    public EventCatalogDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = GetConnectionString(args, configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Pass it via --connection argument or " +
                "set ConnectionStrings__DefaultConnection environment variable.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<EventCatalogDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new EventCatalogDbContext(optionsBuilder.Options);
    }

    private static string? GetConnectionString(string[] args, IConfiguration configuration)
    {
        // Priority 1: Connection string passed via --connection argument
        if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
        {
            return args[0];
        }

        // Priority 2: Environment variable (GitHub Actions sets this)
        var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            return envConnectionString;
        }

        // Priority 3: appsettings.json (for local development)
        return configuration.GetConnectionString("DefaultConnection");
    }
}