using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductGrpcService.Worker
{
    public class MultiExchangeConsumerWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MultiExchangeConsumerWorker> _logger;

        public MultiExchangeConsumerWorker(IConfiguration configuration, ILogger<MultiExchangeConsumerWorker> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration.GetConnectionString("RabbitMQ")
                ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");

            var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
            var connection = await factory.CreateConnectionAsync(stoppingToken);

            // Start all 4 consumer loops on the same connection concurrently
            //await SetupDirectConsumerAsync(connection, stoppingToken);
            //await SetupFanoutConsumerAsync(connection, stoppingToken, "q.fanout.queue1");
            //_ = SetupFanoutConsumerAsync(connection, stoppingToken, "q.fanout.queue2"); // 2nd queue proves broadcast!
           await SetupTopicConsumerAsync(connection, stoppingToken);
            //_ = SetupHeadersConsumerAsync(connection, stoppingToken);

            _logger.LogInformation("[MULTI-WORKER] All 4 exchange consumers are up and listening...");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
            }
        }
        private async Task SetupDirectConsumerAsync(IConnection connection, CancellationToken stoppingToken)
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            string exchange = "ex.direct.products";
            string queue = "q.direct.queue";
            string routingKey = "product.update";


            await channel.ExchangeDeclareAsync(
                exchange, 
                ExchangeType.Direct, 
                durable: true, 
                cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(
                queue, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                cancellationToken: stoppingToken);
            await channel.QueueBindAsync(queue, exchange, routingKey, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[DIRECT CONSUMER] Received: {Message}", msg);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        }






        private async Task SetupFanoutConsumerAsync(IConnection connection, CancellationToken stoppingToken, string queueName)
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            string exchange = "ex.fanout.notifications";

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueBindAsync(queueName, exchange, routingKey: "", cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[FANOUT CONSUMER - {Queue}] Received broadcast: {Message}", queueName, msg);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        }
        private async Task SetupTopicConsumerAsync(IConnection connection, CancellationToken stoppingToken)
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            string exchange = "ex.topic.catalog";
            string queue = "q.topic.europe.queue";
            string bindingPattern = "product.europe.*"; // Matches anything starting with product.europe.

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueBindAsync(queue, exchange, bindingPattern, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[TOPIC CONSUMER] Received (Key: {Key}): {Message}", ea.RoutingKey, msg);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        }
        private async Task SetupHeadersConsumerAsync(IConnection connection, CancellationToken stoppingToken)
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            string exchange = "ex.headers.metadata";
            string queue = "q.headers.queue";

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Headers, durable: true, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            var bindingArgs = new Dictionary<string, object>
            {
                { "x-match", "all" }, // Requires ALL headers to match
                { "format", "json" },
                { "region", "asia" }
            };

            await channel.QueueBindAsync(queue, exchange, routingKey: "", arguments: bindingArgs, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var msg = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[HEADERS CONSUMER] Received matching header attributes: {Message}", msg);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        }
    }
}
