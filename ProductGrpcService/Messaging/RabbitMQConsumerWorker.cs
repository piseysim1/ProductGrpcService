using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProductGrpcService.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductGrpcService.Messaging
{
    public class RabbitMQConsumerWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RabbitMQConsumerWorker> _logger;

        private const string QueueName = "product_queue";

        public RabbitMQConsumerWorker(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<RabbitMQConsumerWorker> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
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
                await factory.CreateConnectionAsync(stoppingToken);

            await using var channel =
                await connection.CreateChannelAsync(
                    cancellationToken: stoppingToken);

            // Declare DLQ
            await channel.QueueDeclareAsync(
                queue: "product_dlq",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            var arguments = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = "",
                ["x-dead-letter-routing-key"] = "product_dlq"
            };

            // Declare main queue
            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: arguments,
                cancellationToken: stoppingToken);

            // Receive one unacknowledged message at a time
            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    // Convert message to JSON
                    var json = System.Text.Encoding.UTF8.GetString(
                        ea.Body.ToArray());

                    var command =
                        JsonSerializer.Deserialize<UpdateProductCommand>(json);

                    if (command == null)
                    {
                        throw new Exception("Invalid product command.");
                    }

                    _logger.LogInformation(
                        "[WORKER] Processing Product ID: {Id}",
                        command.ProductId);

                    // TEST: Simulate failure
                    if (command.Name.Contains(
                        "[FAIL]",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(
                            "Simulated processing failure!");
                    }

                    // Create a new DI scope for DbContext
                    using var scope = _scopeFactory.CreateScope();

                    var db = scope.ServiceProvider
                        .GetRequiredService<SampleDbContext>();

                    // Find product in PostgreSQL
                    var product = await db.Products.FindAsync(
                        new object[] { command.ProductId },
                        stoppingToken);

                    if (product == null)
                    {
                        throw new Exception(
                            $"Product ID {command.ProductId} not found.");
                    }

                    // Update product
                    product.Name = command.Name;
                    product.Description = command.Description;
                    product.Price = command.Price;
                    product.CategoryId = command.CategoryId;
                    product.InventoryId = command.InventoryId;

                    // Save to PostgreSQL
                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "[WORKER] Product {Id} updated successfully.",
                        product.Id);

                    // ACK: Database update succeeded
                    await channel.BasicAckAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false);

                    _logger.LogInformation(
                        "[ACK] Message acknowledged.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[WORKER] Failed to update product.");

                    // NACK: Send failed message to DLQ
                    await channel.BasicNackAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    _logger.LogWarning(
                        "[NACK] Message sent to Dead Letter Queue.");
                }
            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                "[WORKER] Listening to RabbitMQ queue: {Queue}",
                QueueName);

            try
            {
                await Task.Delay(
                    Timeout.Infinite,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
            }
        }
    }
}