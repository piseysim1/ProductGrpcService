
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ProductGrpcService.Observability;

public static class Telemetry
{
    public const string ActivitySourceName = "ProductGrpcService";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);

    public const string MeterName = "ProductGrpcService";

    public static readonly Meter Meter = new(MeterName);

    // Count total gRPC requests
    public static readonly Counter<long> GrpcRequests =
        Meter.CreateCounter<long>(
            "grpc.product.requests",
            unit: "{request}",
            description: "Total ProductService gRPC requests"
        );

    // Measure gRPC operation duration
    public static readonly Histogram<double> GrpcDuration =
        Meter.CreateHistogram<double>(
            "grpc.product.duration",
            unit: "ms",
            description: "ProductService gRPC operation duration"
        );
}
