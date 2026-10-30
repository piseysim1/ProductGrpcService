using ProductGrpcService.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapAllGrpcServices();

app.Run();