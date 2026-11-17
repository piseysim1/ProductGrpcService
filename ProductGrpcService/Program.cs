using AutoMapper;
using ProductGrpcService;
using ProductGrpcService.Config;
using ProductGrpcService.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<SampleDbContext>();

// Configure Serilog logging
builder.Configuration.AddJsonFile("appsettings.json")
                     .AddEnvironmentVariables();
SerilogConfig.ConfigureSerilog(builder);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure and register AutoMapper
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var config = new MapperConfiguration(cfg => {
    cfg.AddProfile<AutoMapperConfig>();
}, loggerFactory);
IMapper mapper = config.CreateMapper();
builder.Services.AddSingleton(mapper);

// Add OpenTelemetry configuration
builder.ConfigureOpenTelemetry();

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

using var scope = app.Services.CreateScope();

// Map your gRPC services
app.MapAllGrpcServices();

app.Run();