using Grpc.Net.Client;
using gRoom.gRPC.Messages;
using Google.Protobuf.WellKnownTypes;

using var channel = GrpcChannel.ForAddress("http://localhost:5087");
var client = new Groom.GroomClient(channel);

Console.WriteLine("*** Admin Console started ***");
Console.WriteLine("Listening...");

// ADD THE MONITORING CODE BELOW THIS LINE
using var call = client.StartMonitoring(new Empty());
var cts = new CancellationTokenSource();

while (await call.ResponseStream.MoveNext(cts.Token)) {
    var msg = call.ResponseStream.Current;
    Console.WriteLine($"New message: {msg.Content}, user: {msg.User}, at: {msg.MsgTime}");
}