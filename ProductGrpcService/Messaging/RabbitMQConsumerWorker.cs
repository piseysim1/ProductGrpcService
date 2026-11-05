using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductGrpcService.Messaging
{
    public class RabbitMQConsumerWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel; 

        public RabbitMQConsumerWorker(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_configuration.GetConnectionString("RabbitMQ")!)
            };

            // v7 uses asynchronous connection creation
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Declare the queue asynchronously
            await _channel.QueueDeclareAsync(
                queue: "product_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    Console.WriteLine($"Consumer received: {message}");

                    if (message.Contains("fail"))
                    {
                        throw new Exception("Simulated processing failure!");
                    }

                   
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    Console.WriteLine("Message processed and ACKed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    Console.WriteLine("Message rejected (NACK).");
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "product_queue",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null) await _channel.CloseAsync(cancellationToken: cancellationToken);
            if (_connection != null) await _connection.CloseAsync(cancellationToken: cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}