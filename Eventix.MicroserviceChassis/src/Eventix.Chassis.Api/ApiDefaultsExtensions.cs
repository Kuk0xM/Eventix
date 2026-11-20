using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Eventix.Chassis.Api;

public static class ApiDefaultsExtensions
{
    /// <summary>
    /// Configures ALL standard API boilerplate in one call:
    /// Serilog, OpenTelemetry, Health Checks, Forwarded Headers, OpenAPI.
    /// </summary>
    public static WebApplicationBuilder AddEventixApiDefaults(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        // 1. Serilog
        builder.Host.UseEventixSerilog(serviceName);

        // 2. OpenTelemetry
        builder.Services.AddEventixTelemetry(builder.Configuration, serviceName);

        // 3. Health Checks
        builder.Services.AddEventixHealthChecks(builder.Configuration);

        // 4. Forwarded Headers (Azure Container Apps proxy)
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // 5. OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        return builder;
    }

    /// <summary>
    /// Configures ALL standard middleware pipeline in one call:
    /// Forwarded Headers, Serilog Request Logging, OpenAPI, HTTPS Redirection, Health Check endpoints.
    /// </summary>
    public static WebApplication UseEventixApiDefaults(this WebApplication app)
    {
        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        // Health check endpoints
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false // liveness: no dependencies
        });
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}