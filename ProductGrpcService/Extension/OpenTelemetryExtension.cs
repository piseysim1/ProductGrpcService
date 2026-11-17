using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductGrpcService.Observability;

namespace ProductGrpcService.Extension;

public static class OpenTelemetryExtension
{
    public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        const string serviceName = "ProductGrpcService";

        // Pulls endpoint from appsettings.json, defaults to http://localhost:4317 if missing
        var otlpEndpoint = new Uri(
            builder.Configuration["OpenTelemetry:Endpoint"]
            ?? "http://localhost:4317"
        );

        builder.Services.AddOpenTelemetry()
// Service metadata attached to all telemetry
            .ConfigureResource(resource => resource.AddService(serviceName))

            // 1. TRACES
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(Telemetry.ActivitySourceName) // For custom ActivitySources

                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            })

            // 2. METRICS
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(Telemetry.MeterName) // For custom Meters

                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        // 3. LOGS
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;

            logging.AddOtlpExporter(options =>
            {
                options.Endpoint = otlpEndpoint;
                options.Protocol = OtlpExportProtocol.Grpc;
            });
        });
    }
}