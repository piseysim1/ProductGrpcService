using Serilog;
using Serilog.Sinks.OpenTelemetry;
namespace ProductGrpcService.Config
{
    public static class SerilogConfig
    {
        public static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            var logPath = Path.Combine(
                AppContext.BaseDirectory,
                "logs",
                "log.txt"
            );

            Directory.CreateDirectory(
                Path.GetDirectoryName(logPath)!
            );
            var otlpEndPoint = builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 20 * 1024 * 1024
                )
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndPoint;
                    options.Protocol = OtlpProtocol.Grpc;
                })
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);
        }
    }
}
