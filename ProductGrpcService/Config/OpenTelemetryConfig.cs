
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductGrpcService.Observability;

namespace ProductGrpcService.Config;

public static class OpenTelemetryConfig
{
    public static void ConfigureOpenTelemetry( WebApplicationBuilder builder)
    {
        const string serviceName = "ProductGrpcService";

        var otlpEndpoint = new Uri(
            builder.Configuration["OpenTelemetry:Endpoint"]
            ?? "http://localhost:4317"
        );

        builder.Services
            .AddOpenTelemetry()

            // Service information
            .ConfigureResource(resource =>
                resource.AddService(serviceName))

            // 1. TRACES
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(Telemetry.ActivitySourceName)

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
                    .AddMeter(Telemetry.MeterName)

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
