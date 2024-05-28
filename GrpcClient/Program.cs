using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Greet;
using Grpc.Core;
using Grpc.Net.Client;
using Notify;
using System.Net.Http.Headers;
using System.Text;
using Ticket;
using Weather;

namespace GrpcDemoClient
{
    public class Program
    {
        public const string Address = "https://localhost:7101";
        static async Task Main(string[] args)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var channel = GrpcChannel.ForAddress(Address, new GrpcChannelOptions { HttpHandler = httpHandler });

            #region comparison example

            //var data = new List<(string message, int grpcByteSize, int restByteSize)>();
            //var firstGrpcMessage = await SayNumber(channel);
            //var firstRestMessage = await GetRestMessage("number");
            //var firstGrpcMessageLength = GetGrpcByteLength(firstGrpcMessage);
            //var firstRestMessageLength = await GetRestByteLength(firstRestMessage);
            //data.Add(("1", firstGrpcMessageLength, firstRestMessageLength));
            //var secondGrpcMessage = await SayHello(channel);
            //var secondRestMessage = await GetRestMessage("greet?name=World!");
            //var secondGrpcMessageLength = GetGrpcByteLength(secondGrpcMessage);
            //var secondRestMessageLength = await GetRestByteLength(secondRestMessage);
            //data.Add(("Hello World!", secondGrpcMessageLength, secondRestMessageLength));
            //var thirdGrpcMessage = await GetWeather(channel);
            //var thirdRestMessage = await GetRestMessage("weather");
            //var thirdGrpcMessageLength = GetGrpcByteLength(thirdGrpcMessage);
            //var thirdRestMessageLength = await GetRestByteLength(thirdRestMessage);
            //data.Add(("Weather reply object", thirdGrpcMessageLength, thirdRestMessageLength));
            //PrintOut(data);

            #endregion

            #region streaming example

            //await StreamingExample(channel);

            #endregion

            #region Authorization example
            

            var client = new Ticketer.TicketerClient(channel);

            Console.WriteLine("gRPC Ticketer");
            Console.WriteLine();
            Console.WriteLine("Press a key:");
            Console.WriteLine("1: Get available tickets");
            Console.WriteLine("2: Purchase ticket");
            Console.WriteLine("3: Authenticate");
            Console.WriteLine("4: Exit");
            Console.WriteLine();

            string? token = null;

            var exiting = false;
            while (!exiting)
            {
                var consoleKeyInfo = Console.ReadKey(intercept: true);
                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        await GetAvailableTickets(client);
                        break;
                    case '2':
                        await PurchaseTicket(client, token);
                        break;
                    case '3':
                        token = Authenticate();
                        break;
                    case '4':
                        exiting = true;
                        break;
                }
            }

            #endregion

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

        #region Comparison example
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
        #endregion

        #region Streaming example
        private static async Task StreamingExample(GrpcChannel channel)
        {
            var client = new Notifier.NotifierClient(channel);
            using var call = client.ChatNotification();

            var responseReaderTask = Task.Run(async Task () =>
            {
                //opatovny read zo serveru cez stream
                while (await call.ResponseStream.MoveNext())
                {
                    var note = call.ResponseStream.Current;
                    Console.WriteLine($"{note.Message}, received at {note.ReceivedAt}");
                }
            });

            foreach (var msg in new[] { "Tom", "John" })
            {
                var request = new NotificationsRequest()
                { Message = $"Hello {msg}", From = "Filip", To = msg };
                //opatovny call cez rovnaky stream na server
                await call.RequestStream.WriteAsync(request);
            }

            await call.RequestStream.CompleteAsync();
            await responseReaderTask;
        }
        #endregion

        #region Authorization example

        private static async Task PurchaseTicket(Ticketer.TicketerClient client, string? token)
        {
            Console.WriteLine("Purchasing ticket...");
            try
            {
                Metadata? headers = null;
                if (token != null)
                {
                    headers = new Metadata();
                    headers.Add("Authorization", $"Bearer {token}");
                }

                var response = await client.BuyTicketsAsync(new BuyTicketsRequest { Count = 1 }, headers);
                if (response.Success)
                {
                    Console.WriteLine("Purchase successful.");
                }
                else
                {
                    Console.WriteLine("Purchase failed. No tickets available.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error purchasing ticket." + Environment.NewLine + ex.ToString());
            }
        }

        private static async Task GetAvailableTickets(Ticketer.TicketerClient client)
        {
            Console.WriteLine("Getting available ticket count...");
            var response = await client.GetAvailableTicketsAsync(new Empty());
            Console.WriteLine("Available ticket count: " + response.Count);
        }

        private static string Authenticate()
        {
            var token = "";
            return token;
        }

        #endregion
    }
}