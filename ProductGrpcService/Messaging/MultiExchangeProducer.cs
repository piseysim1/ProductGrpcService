using RabbitMQ.Client;
using System.Text.Json;

namespace ProductGrpcService.Messaging
{
    public class MultiExchangeProducer
    {
        private readonly IConfiguration _configuration;

        public MultiExchangeProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private async Task<IChannel> GetChannelAsync()
        {
            var connectionString = _configuration.GetConnectionString("RabbitMQ")
                ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");

            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            var connection = await factory.CreateConnectionAsync();
            return await connection.CreateChannelAsync();
        }
        

        public async Task PublishDirectAsync(UpdateProductCommand command, string routingKey)
        {
            await using var channel = await GetChannelAsync();
            string exchangeName = "ex.direct.products";

            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true);

            var body = JsonSerializer.SerializeToUtf8Bytes(command);
            var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

            await channel.BasicPublishAsync(exchangeName, routingKey, mandatory: true, basicProperties: properties, body: body);
            Console.WriteLine($"[DIRECT] Published to exchange '{exchangeName}' with key '{routingKey}'");
        }
       

        public async Task PublishFanoutAsync(UpdateProductCommand command)
        {
            await using var channel = await GetChannelAsync();
            string exchangeName = "ex.fanout.notifications";

            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true);

            var body = JsonSerializer.SerializeToUtf8Bytes(command);
            var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

            // Routing key is empty string because fanout ignores it
            await channel.BasicPublishAsync(exchangeName, routingKey: "", mandatory: false, basicProperties: properties, body: body);
            Console.WriteLine($"[FANOUT] Broadcasted message to exchange '{exchangeName}'");
        }
      

        public async Task PublishTopicAsync(UpdateProductCommand command, string routingKey)
        {
            await using var channel = await GetChannelAsync();
            string exchangeName = "ex.topic.catalog";

            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic, durable: true);

            var body = JsonSerializer.SerializeToUtf8Bytes(command);
            var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

            await channel.BasicPublishAsync(exchangeName, routingKey, mandatory: true, basicProperties: properties, body: body);
            Console.WriteLine($"[TOPIC] Published to exchange '{exchangeName}' with key '{routingKey}'");
        }
        

        public async Task PublishHeadersAsync(UpdateProductCommand command, Dictionary<string, object> headers)
        {
            await using var channel = await GetChannelAsync();
            string exchangeName = "ex.headers.metadata";

            await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Headers, durable: true);

            var body = JsonSerializer.SerializeToUtf8Bytes(command);
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = headers
            };
            // Routing key is ignored; headers dictionary handles routing logic
            await channel.BasicPublishAsync(exchangeName, routingKey: "", mandatory: true, basicProperties: properties, body: body);
            Console.WriteLine($"[HEADERS] Published with custom headers to exchange '{exchangeName}'");
        }

    }

}
