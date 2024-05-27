using GrpcDemoRESTService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/greet", (string name) =>
{
    var result = new HelloReply { Message = $"Hello {name}" };
    return Results.Ok(result);
})
.WithName("greet")
.WithOpenApi();

app.MapGet("/number", () =>
{
    var result = new NumberReply { Number = 1 };
    return Results.Ok(result);
})
.WithName("number")
.WithOpenApi();

app.MapGet("/weather", () =>
{
    var wi = new WeatherInfo
    {
        Code = WindDirection.N,
        Pressure = 995,
        Temperature = 15,
        WindSpeed = 10,
        Status = WeatherStatus.CLOUDS
    };
    wi.Warnning.Add($"First info {DateTime.Now.Ticks}");
    wi.Warnning.Add($"Second info {DateTime.Now.Ticks}");
    var forecast = new Dictionary<int, WeatherInfo>();
    forecast[1] = wi;
    forecast[2] = wi;
    forecast[3] = wi;

    var reply = new WeatherReply();
    reply.Actual = wi;
    reply.Forecast = forecast;

    return Results.Ok(reply);
})
.WithName("weather")
.WithOpenApi();

app.Run();
