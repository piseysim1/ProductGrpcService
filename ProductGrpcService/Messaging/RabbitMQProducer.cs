using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace ProductGrpcService.Messaging
{
    public class RabbitMQProducer
    {
        private readonly IConfiguration _configuration;

        public RabbitMQProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async void PublishProductEvent(string message)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_configuration.GetConnectionString("RabbitMQ")!)
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "product_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "product_queue",
                body: body
            );

            Console.WriteLine($" [x] Published Event to RabbitMQ: {message}");
        }
    }
}