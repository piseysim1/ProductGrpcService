using AutoMapper;
using ProductGrpcService;
using ProductGrpcService.Config;
using ProductGrpcService.Extension;
using ProductGrpcService.Messaging;
using ProductGrpcService.Worker;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SampleDbContext>();

// config seriod log 
builder.Configuration.AddJsonFile("appsettings.json")
                     .AddEnvironmentVariables();
SerilogConfig.ConfigureSerilog(builder);
// add config cros 

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

// add config opentelemetry  
OpenTelemetryConfig.ConfigureOpenTelemetry(builder);

builder.Services.AddGrpc();
var app = builder.Build();

using var scope = app.Services.CreateScope();
app.MapAllGrpcServices();

app.Run();