using GrpcDemoService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<WeatherService>();
app.MapGrpcService<NotifierService>();
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}
app.Run();
