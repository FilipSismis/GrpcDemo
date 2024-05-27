using Google.Protobuf;
using Greet;
using Grpc.Net.Client;
using Weather;
using System.Net.Http.Headers;
using System.Text;
using Google.Protobuf.WellKnownTypes;

namespace GrpcDemoClient
{
    public class Program
    {
        public const string Address = "https://localhost:7101";
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress(Address);

            
            var data = new List<(string message, int grpcByteSize, int restByteSize)>();
            var firstGrpcMessage = await SayNumber(channel);
            var firstRestMessage = await GetRestMessage("number");
            var firstGrpcMessageLength = GetGrpcByteLength(firstGrpcMessage);
            var firstRestMessageLength = await GetRestByteLength(firstRestMessage);
            data.Add(("1", firstGrpcMessageLength, firstRestMessageLength));
            var secondGrpcMessage = await SayHello(channel);
            var secondRestMessage = await GetRestMessage("greet?name=World!");
            var secondGrpcMessageLength = GetGrpcByteLength(secondGrpcMessage);
            var secondRestMessageLength = await GetRestByteLength(secondRestMessage);
            data.Add(("Hello World!", secondGrpcMessageLength, secondRestMessageLength));
            var thirdGrpcMessage = await GetWeather(channel);
            var thirdRestMessage = await GetRestMessage("weather");
            var thirdGrpcMessageLength = GetGrpcByteLength(thirdGrpcMessage);
            var thirdRestMessageLength = await GetRestByteLength(thirdRestMessage);
            data.Add(("Weather reply object", thirdGrpcMessageLength, thirdRestMessageLength));
            PrintOut(data);
            Console.ReadKey();
        }

        #region printing
        private static void PrintOut(List<(string message, int grpcByteSize, int restByteSize)> data)
        {
            PrintRow("Message", "GRPC byte size", "REST byte size");
            PrintLine();

            // Print rows
            foreach (var (message, grpcByteSize, restByteSize) in data)
            {
                PrintRow(message, grpcByteSize.ToString(), restByteSize.ToString());
            }
        }

        static void PrintLine()
        {
            Console.WriteLine(new string('-', 100));
        }

        static void PrintRow(params string[] columns)
        {
            int width = (100 - columns.Length) / columns.Length;
            const string separator = " | ";

            string row = "| " + string.Join(separator, columns.Select(c => AlignCentre(c, width))) + " |";
            Console.WriteLine(row);
        }

        static string AlignCentre(string text, int width)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;
                int padding = width - text.Length;
                int padLeft = padding / 2 + text.Length;
                return text.PadLeft(padLeft).PadRight(width);
            }
        }
        #endregion

        private static async Task<HelloReply> SayHello(GrpcChannel channel)
        {
            var client = new Greeter.GreeterClient(channel);
            HelloRequest request = new HelloRequest { Name = "World!" };
            return await client.SayHelloAsync(request);
        }

        private static async Task<NumberReply> SayNumber(GrpcChannel channel)
        {
            var client = new Greeter.GreeterClient(channel);
            var request = new Empty();
            return await client.SayNumberAsync(request);

        }

        private static async Task<WeatherReply> GetWeather(GrpcChannel channel)
        {
            var client = new WeatherGetter.WeatherGetterClient(channel);
            var request = new Empty();
            return await client.GetWeatherAsync(request);
        }

        private static int GetGrpcByteLength(IMessage message)
        {
            return message.CalculateSize();
        }

        private static async Task<HttpResponseMessage> GetRestMessage(string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:5023/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                return await client.GetAsync(path);
            }
        }

        private async static Task<int> GetRestByteLength(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var byteArray = Encoding.UTF8.GetBytes(jsonContent);
                return byteArray.Length;
            }
            return 0;
        }
    }
}