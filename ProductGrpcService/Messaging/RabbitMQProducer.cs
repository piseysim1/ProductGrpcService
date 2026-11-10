using System.Text.Json;
using RabbitMQ.Client;

namespace ProductGrpcService.Messaging
{
    public class RabbitMQProducer
    {
        private readonly IConfiguration _configuration;

        private const string QueueName = "product_queue";
        private const string DeadLetterQueue = "product_dlq";

        public RabbitMQProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task PublishProductUpdateAsync(
            UpdateProductCommand command)
        {
            var connectionString =
                _configuration.GetConnectionString("RabbitMQ")
                ?? throw new InvalidOperationException(
                    "RabbitMQ connection string is missing.");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            await using var connection =
                await factory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            
            await channel.QueueDeclareAsync(
                queue: DeadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            
            var arguments = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = "",
                ["x-dead-letter-routing-key"] = DeadLetterQueue
            };

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments);

            
            var body = JsonSerializer.SerializeToUtf8Bytes(command);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body);

            Console.WriteLine(
                $"public message the product update: {command.ProductId}");
        }
    }   
}