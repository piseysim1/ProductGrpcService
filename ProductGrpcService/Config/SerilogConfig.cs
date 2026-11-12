using Serilog;
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
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);
        }
    }
}
