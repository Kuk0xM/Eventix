using Eventix.Chassis.Api;
using Eventix.EventCatalog.API.Endpoints;
using Eventix.EventCatalog.Application;
using Eventix.EventCatalog.Infrastructure;
using Serilog;

Log.Logger = SerilogConfiguration.CreateBootstrapLogger("EventCatalog");

try
{
    Log.Information("Starting Eventix.EventCatalog API");

    var builder = WebApplication.CreateBuilder(args);

    // ONE LINE: Serilog + OpenTelemetry + HealthChecks + ForwardedHeaders + OpenAPI
    builder.AddEventixApiDefaults("EventCatalog");

    // Service-specific layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // ONE LINE: ForwardedHeaders + RequestLogging + OpenAPI + HTTPS + /health/live + /health/ready
    app.UseEventixApiDefaults();

    // Service-specific endpoints
    app.MapEventEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}