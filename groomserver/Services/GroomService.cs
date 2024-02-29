using Grpc.Core;
using gRoom.gRPC.Messages;
using Google.Protobuf.WellKnownTypes;
using gRoom.gRPC.Utils;

namespace gRoom.gRPC.Services;

public class GroomService : Groom.GroomBase
{
    private readonly ILogger<GroomService> _logger;
    public GroomService(ILogger<GroomService> logger)
    {
        _logger = logger;
    }

    public override async Task<RoomRegistrationResponse> RegisterToRoom(RoomRegistrationRequest request, ServerCallContext context)
    {
        await Task.Delay(10000);

        // _logger.LogInformation("Service called...");


        // var rnd = new Random();
        // var roomNum = rnd.Next(1, 100);
        // _logger.LogInformation($"Room no. {roomNum}");
        // var resp = new RoomRegistrationResponse { RoomId = roomNum };
        // return Task.FromResult(resp);

        UsersQueues.CreateUserQueue(request.RoomName, request.UserName);
        var resp = new RoomRegistrationResponse() { Joined = true };
        return await Task.FromResult(resp);
    }

    public override async Task<NewsStreamStatus> SendNewsFlash(IAsyncStreamReader<NewsFlash> newsStream, ServerCallContext context)
    {
        while (await newsStream.MoveNext())
        {
            var news = newsStream.Current;
            MessagesQueue.AddNewsToQueue(news);
            Console.WriteLine($"News flash: {news.NewsItem}");
        }

        return new NewsStreamStatus { Success = true };
    }

    public override async Task StartMonitoring(Empty request, IServerStreamWriter<ReceivedMessage> streamWriter, ServerCallContext context)
    {
        while (true)
        {
            //await streamWriter.WriteAsync(new ReceivedMessage { MsgTime = Timestamp.FromDateTime(DateTime.UtcNow), User = "1", Content = "Test msg" });
            if (MessagesQueue.GetMessagesCount() > 0)
            {
                await streamWriter.WriteAsync(MessagesQueue.GetNextMessage());
            }
            if (UsersQueues.GetAdminQueueMessageCount()>0) {
                await streamWriter.WriteAsync(UsersQueues.GetNextAdminMessage());
            }
            await Task.Delay(1000);
        }

    }

    public override async Task StartChat(IAsyncStreamReader<ChatMessage> incomingStream, IServerStreamWriter<ChatMessage> outgoingStream, ServerCallContext context)
    {

        // Wait for the first message to get the user name
        while (!await incomingStream.MoveNext())
        {
            await Task.Delay(100);
        }

        string userName = incomingStream.Current.User;
        string room = incomingStream.Current.Room;
        Console.WriteLine($"User {userName} connected to room {incomingStream.Current.Room}");

        // TEST TEST TEST TEST - TO USE ONLY WHEN TESTING WITH BLOOMRPC
        //We dont need this line because user logins alone with fullroomclient
        //UsersQueues.CreateUserQueue(room, userName);
        // END TEST END TEST END TEST

        // Get messages from the user
        var reqTask = Task.Run(async () =>
        {
            while (await incomingStream.MoveNext())
            {
                Console.WriteLine($"Message received: {incomingStream.Current.Content}");
                UsersQueues.AddMessageToRoom(ConvertToReceivedMessage(incomingStream.Current), incomingStream.Current.Room);
            }
        });


        // Check for messages to send to the user
        var respTask = Task.Run(async () =>
        {
            while (true)
            {
                var userMsg = UsersQueues.GetMessageForUser(userName);
                if (userMsg != null)
                {
                    var userMessage = ConvertToChatMessage(userMsg, room);
                    await outgoingStream.WriteAsync(userMessage);
                }
                if (MessagesQueue.GetMessagesCount() > 0)
                {
                    var news = MessagesQueue.GetNextMessage();
                    var newsMessage = ConvertToChatMessage(news, room);
                    await outgoingStream.WriteAsync(newsMessage);
                }

                await Task.Delay(200);
            }
        });

        // Keep the method running
        while (true)
        {
            await Task.Delay(10000);
        }
    }


    private ReceivedMessage ConvertToReceivedMessage(ChatMessage chatMsg)
    {
        var rcMsg = new ReceivedMessage();
        rcMsg.Content = chatMsg.Content;
        rcMsg.MsgTime = chatMsg.MsgTime;
        rcMsg.User = chatMsg.User;

        return rcMsg;
    }

    private ChatMessage ConvertToChatMessage(ReceivedMessage rcMsg, string room)
    {
        var chatMsg = new ChatMessage();
        chatMsg.Content = rcMsg.Content;
        chatMsg.MsgTime = rcMsg.MsgTime;
        chatMsg.User = rcMsg.User;
        chatMsg.Room = room;

        return chatMsg;
    }

}
