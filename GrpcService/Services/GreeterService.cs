using Google.Protobuf.WellKnownTypes;
using Greet;
using Grpc.Core;

namespace GrpcDemoService.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<NumberReply> SayNumber(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new NumberReply
            {
                Number = 1
            });
        }
    }
}
