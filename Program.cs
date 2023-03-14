using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Define attributes for your application
var resourceBuilder = ResourceBuilder.CreateDefault()
    // add attributes for the name and version of the service
    .AddService("MyCompany.MyProduct.LogsDemoAPI", serviceVersion: "1.0.0")
    // add attributes for the OpenTelemetry SDK version
    .AddTelemetrySdk()
    // add custom attributes
    .AddAttributes(new Dictionary<string, object>
    {
        ["host.name"] = Environment.MachineName,
        ["os.description"] = RuntimeInformation.OSDescription,
        ["deployment.environment"] = builder.Environment.EnvironmentName.ToLowerInvariant(),
    });

builder.Logging.ClearProviders()
    .AddOpenTelemetry(loggerOptions =>
    {
        loggerOptions
            // define the resource
            .SetResourceBuilder(resourceBuilder)
            // add custom processor
            .AddProcessor(new CustomLogProcessor())
            // send logs to the console using exporter
            .AddConsoleExporter();

        loggerOptions.IncludeFormattedMessage = true;
        loggerOptions.IncludeScopes = true;
        loggerOptions.ParseStateValues = true;
    });

var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Hello user!");
    return Results.Ok();
});

app.MapPost("/login", (ILogger<Program> logger, [FromBody] LoginData data) =>
{
    logger.LogInformation("User login attempted: Username {Username}, Password {Password}", data.Username,
        data.Password);
    logger.LogWarning("User login failed: Username {Username}", data.Username);
    return Results.Unauthorized();
});

app.Run();

internal record LoginData(string Username, string Password);