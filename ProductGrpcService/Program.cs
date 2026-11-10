using AutoMapper;
using ProductGrpcService;
using ProductGrpcService.Config;
using ProductGrpcService.Extension;
using ProductGrpcService.Messaging;
using ProductGrpcService.Worker;

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
//builder.Services.AddSingleton<ProductGrpcService.Messaging.RabbitMQProducer>();

// Register RabbitMQ Background Consumer Worker
//builder.Services.AddHostedService<ProductGrpcService.Worker.RabbitMQConsumerWorker>();

// Register the Multi-Exchange Producer
//builder.Services.AddSingleton<ProductGrpcService.Messaging.MultiExchangeProducer>();

// Register the Multi-Exchange Consumer Worker
//builder.Services.AddHostedService<ProductGrpcService.Worker.MultiExchangeConsumerWorker>();
builder.Services.AddSingleton<StreamProducer>();

//builder.Services.AddHostedService<RabbitMQStreamWorker>();
builder.Services.AddGrpc();
var app = builder.Build();
// --- Test RabbitMQ Publisher End-to-End ---

app.MapGet("/test/stream", async (
    StreamProducer producer) =>
{
    await producer.PublishTestMessagesAsync(5);

    return Results.Ok(new
    {
        Message = "Successfully published 5 messages!",
        Stream = "product-events"
    });
});
using var scope = app.Services.CreateScope();
app.MapAllGrpcServices();

app.Run();