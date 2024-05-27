using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Weather;

namespace GrpcDemoService.Services
{
    public class WeatherService : WeatherGetter.WeatherGetterBase
    {
        public override Task<WeatherReply> GetWeather(Empty request, ServerCallContext context)
        {
            var wi = new WeatherInfo
            {
                Code = WindDirection.N,
                Pressure = 995,
                Temperature = 15,
                WindSpeed = 10,
                Status = WeatherStatus.Clouds
            };
            wi.Warnning.Add($"First info {DateTime.Now.Ticks}");
            wi.Warnning.Add($"Second info {DateTime.Now.Ticks}");
            var forecast = new Dictionary<int, WeatherInfo>();
            forecast[1] = wi;
            forecast[2] = wi;
            forecast[3] = wi;

            var reply = new WeatherReply();
            reply.Actual = wi;
            reply.Forecast.Add(forecast);

            return Task.FromResult(reply);
        }
    }
}
