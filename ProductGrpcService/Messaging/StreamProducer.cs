
using System.Net;
using System.Text;
using System.Text.Json;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

namespace ProductGrpcService.Messaging;

public class StreamProducer
{
    private const string StreamName = "product-events";

    public async Task PublishTestMessagesAsync(int count = 5)
    {
        // 1. Connect to RabbitMQ Stream port 5552.
        var system = await StreamSystem.Create(
            new StreamSystemConfig
            {
                UserName = "guest",
                Password = "guest",
                Endpoints = new List<EndPoint>
                {
                    new IPEndPoint(IPAddress.Loopback, 5552)
                }
            });

        try
        {
            // 2. Create the Stream if it doesn't exist.
            await system.CreateStream(
                new StreamSpec(StreamName)
                {
                    MaxLengthBytes = 100_000_000
                });

            // 3. Create a Producer.
            var producer = await Producer.Create(
                new ProducerConfig(system, StreamName));

            try
            {
                // 4. Send test messages.
                for (int i = 1; i <= count; i++)
                {
                    var product = new
                    {
                        EventType = "ProductCreated",
                        ProductId = i,
                        ProductName = $"Test Product {i}",
                        Price = 100 + i,
                        CreatedAt = DateTime.UtcNow
                    };

                    string json = JsonSerializer.Serialize(product);

                    var message = new Message(
                        Encoding.UTF8.GetBytes(json));

                    await producer.Send(message);

                    Console.WriteLine(
                        $"[STREAM PRODUCER] Sent: {json}");
                }
            }
            finally
            {
                await producer.Close();
            }
        }
        finally
        {
            await system.Close();
        }
    }
}
