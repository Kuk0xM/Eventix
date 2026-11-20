using Eventix.Chassis.Api;
using Eventix.Booking.Application;
using Eventix.Booking.Infrastructure;
using Serilog;

Log.Logger = SerilogConfiguration.CreateBootstrapLogger("Booking");

try
{
    Log.Information("Starting Eventix.Booking API");

    var builder = WebApplication.CreateBuilder(args);

    // ONE LINE: Serilog + OpenTelemetry + HealthChecks + ForwardedHeaders + OpenAPI
    builder.AddEventixApiDefaults("Booking");

    builder.Services.AddControllers();

    // Service-specific layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // ONE LINE: middleware pipeline + health endpoints
    app.UseEventixApiDefaults();

    app.MapControllers();

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