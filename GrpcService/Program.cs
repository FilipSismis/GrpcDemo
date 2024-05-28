using GrpcDemoService.Services;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using GrpcDemoService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddSingleton<TicketRepository>();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("Entra"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<WeatherService>();
app.MapGrpcService<NotifierService>();
app.MapGrpcService<TicketerService>();

app.UseAuthentication();
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}
app.Run();
