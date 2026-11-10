
using System.Net;
using System.Text;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

namespace ProductGrpcService.Worker;

public class RabbitMQStreamWorker : BackgroundService
{
    private const string StreamName = "product-events";

    private StreamSystem? _system;
    private Consumer? _consumer;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            // 1. Connect to RabbitMQ Stream.
            _system = await StreamSystem.Create(
                new StreamSystemConfig
                {
                    UserName = "guest",
                    Password = "guest",
                    Endpoints = new List<EndPoint>
                    {
                        new IPEndPoint(IPAddress.Loopback, 5552)
                    }
                });

            // 2. Ensure the Stream exists.
            await _system.CreateStream(
                new StreamSpec(StreamName)
                {
                    MaxLengthBytes = 100_000_000
                });

            Console.WriteLine(
                "[STREAM CONSUMER] Connected!");

            // 3. Create a Consumer.
            _consumer = await Consumer.Create(
                new ConsumerConfig(_system, StreamName)
                {
                    // Read from the first available message.
                    OffsetSpec = new OffsetTypeFirst(),

                    MessageHandler = async (
                        stream,
                        consumer,
                        context,
                        message) =>
                    {
                        // 4. Convert message bytes into text.
                        string body = Encoding.UTF8.GetString(
                            message.Data.Contents);

                        Console.WriteLine(
                            $"[STREAM CONSUMER] Stream: {stream}");

                        Console.WriteLine(
                            $"[STREAM CONSUMER] Offset: {context.Offset}");

                        Console.WriteLine(
                            $"[STREAM CONSUMER] Received: {body}");

                        await Task.CompletedTask;
                    }
                });

            Console.WriteLine(
                "[STREAM CONSUMER] Listening...");

            // 5. Keep the worker running.
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        finally
        {
            if (_consumer != null)
                await _consumer.Close();

            if (_system != null)
                await _system.Close();

            Console.WriteLine(
                "[STREAM CONSUMER] Stopped.");
        }
    }
}
