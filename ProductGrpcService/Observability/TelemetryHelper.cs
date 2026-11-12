
using System.Diagnostics;
namespace ProductGrpcService.Observability;

public static class TelemetryHelper
{
    public static async Task<T> TrackAsync<T>(
        string operationName,
        ILogger logger,
        Func<Task<T>> action)
    {
        using var activity =
            Telemetry.ActivitySource.StartActivity(operationName);

        var stopwatch = Stopwatch.StartNew();

        Telemetry.GrpcRequests.Add(
            1,
            new KeyValuePair<string, object>(
                "grpc.method", operationName
            )
        );

        try
        {
            logger.LogInformation(
                "Starting operation: {OperationName}",
                operationName
            );

            var result = await action();

            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation(
                "Operation completed: {OperationName}",
                operationName
            );

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message
            );

            logger.LogError(
                ex,
                "Operation failed: {OperationName}",
                operationName
            );

            throw;
        }
        finally
        {
            stopwatch.Stop();

            Telemetry.GrpcDuration.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object>(
                    "grpc.method", operationName
                )
            );
        }
    }
}
