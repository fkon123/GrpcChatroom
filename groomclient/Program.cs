using Grpc.Net.Client;
using gRoom.gRPC.Messages;

using var channel = GrpcChannel.ForAddress("http://localhost:5087");
var client = new Groom.GroomClient(channel);
Console.Write("Enter room name: ");
var roomName = Console.ReadLine();
var resp = client.RegisterToRoom(new RoomRegistrationRequest {RoomName=roomName});
Console.WriteLine($"Room ID: {resp.RoomId}");
Console.Read();