using AutoMapper;
using ProductGrpcService;
using ProductGrpcService.Config;
using ProductGrpcService.Extension;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SampleDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// add auto mapper 
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var config = new MapperConfiguration(cfg => {
    cfg.AddProfile<AutoMapperConfig>();
}, loggerFactory);
IMapper mapper = config.CreateMapper();
builder.Services.AddSingleton(mapper);
// Register RabbitMQ Producer
builder.Services.AddSingleton<ProductGrpcService.Messaging.RabbitMQProducer>();

// Register RabbitMQ Background Consumer Worker
builder.Services.AddHostedService<ProductGrpcService.Messaging.RabbitMQConsumerWorker>();
builder.Services.AddGrpc();

var app = builder.Build();
// --- Test RabbitMQ Publisher End-to-End ---
using var scope = app.Services.CreateScope();
//var producer = scope.ServiceProvider.GetRequiredService<ProductGrpcService.Messaging.RabbitMQProducer>();

//// Publish a successful test message
//producer.PublishProductEvent("Product ID 1 was updated successfully.");

//// Publish a test message that triggers your consumer's error handling
//producer.PublishProductEvent("Product ID 2 fail trigger test.");
app.MapAllGrpcServices();

app.Run();