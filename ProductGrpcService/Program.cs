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
//builder.Services.AddSingleton<ProductGrpcService.Messaging.RabbitMQProducer>();

// Register RabbitMQ Background Consumer Worker
//builder.Services.AddHostedService<ProductGrpcService.Worker.RabbitMQConsumerWorker>();
builder.Services.AddGrpc();
// Register the Multi-Exchange Producer
builder.Services.AddSingleton<ProductGrpcService.Messaging.MultiExchangeProducer>();

// Register the Multi-Exchange Consumer Worker
builder.Services.AddHostedService<ProductGrpcService.Worker.MultiExchangeConsumerWorker>();

var app = builder.Build();
// --- Test RabbitMQ Publisher End-to-End ---
using var scope = app.Services.CreateScope();
app.MapAllGrpcServices();

app.Run();