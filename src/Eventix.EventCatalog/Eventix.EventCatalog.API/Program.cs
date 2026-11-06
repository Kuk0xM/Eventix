using Eventix.EventCatalog.API.Endpoints;
using Eventix.EventCatalog.API.HealthChecks;
using Eventix.EventCatalog.API.Telemetry;
using Eventix.EventCatalog.Application;
using Eventix.EventCatalog.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "Eventix.EventCatalog")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Eventix.EventCatalog API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Eventix.EventCatalog.API",
            Version = "v1",
            Description = "API for managing events in the Eventix platform"
        });
    });

    // Trust the Azure Container Apps proxy for X-Forwarded-* headers
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // Application layer (MediatR handlers)
    builder.Services.AddApplication();

    // Infrastructure layer (DbContext, Repositories, Outbox, RabbitMQ)
    builder.Services.AddInfrastructure(builder.Configuration);

    // OpenTelemetry (traces + metrics)
    builder.Services.AddEventixTelemetry(builder.Configuration);

    // Health Checks
    builder.Services.AddEventixHealthChecks(builder.Configuration);

    var app = builder.Build();

    app.UseForwardedHeaders();

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Eventix.EventCatalog.API V1");
        });
    }

    app.UseHttpsRedirection();

    // Health check endpoints
    app.MapEventixHealthChecks();

    // API endpoints
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