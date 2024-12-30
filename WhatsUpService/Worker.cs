using System.Net.Sockets;
using System.Net;
using System.Text;
using WhatsUpService.Core.Services;
using WhatsUpService.Core.Models;

public class TcpServerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<string, TcpClient> _connectedClients = new();


    public TcpServerWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int port = 5000;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"Server running on port {port}");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client, stoppingToken);
        }

        listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        using var networkStream = client.GetStream();
        var buffer = new byte[1024];

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                if (bytesRead == 0) break;

                var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var response = await ProcessRequestAsync(request);

                var responseBytes = Encoding.UTF8.GetBytes(response);
                await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length, stoppingToken);
            }
        }
        finally
        {
            client.Close();
        }
    }

    private async Task<string> ProcessRequestAsync(string request)
    {
        if (string.IsNullOrWhiteSpace(request))
            return "Invalid request.";

        var parts = request.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return "Invalid request format.";

        var command = parts[0].ToUpperInvariant();

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var messageService = scope.ServiceProvider.GetRequiredService<MessageService>();
        var friendsService = scope.ServiceProvider.GetRequiredService<FriendsService>();
        var chatService = scope.ServiceProvider.GetRequiredService<ChatService>();

        try
        {
            switch (command)
            {
                case "LOGIN":
                    if (parts.Length < 3)
                        return "Invalid login format. Expected: LOGIN:username:password";
                    return await userService.LoginAsync(parts[1], parts[2]);

                case "SIGN_UP":
                    if (parts.Length < 4)
                        return "Invalid signup format. Expected: SIGNUP:username:password:email";
                    return await userService.SignUpAsync(parts[1], parts[2], parts[3]);

                case "SEND_MESSAGE":
                    if (parts.Length < 4)
                        return "Invalid message format. Expected: SEND_MESSAGE:senderId:chatId:content";
                    if (!int.TryParse(parts[1], out int senderId) || !Guid.TryParse(parts[2], out Guid chatId))
                        return "Invalid IDs. SenderId must be an integer and ChatId must be a valid GUID.";
                    string message = string.Join(':', parts.Skip(3));
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        return "Message cannot be empty.";
                    }
                    await messageService.SaveMessageAsync(senderId, chatId, message);
                    return "Message sent.";

                case "FETCH_FRIENDS":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int userId))
                        return "Invalid request format. Expected: FETCH_FRIENDS:userId";
                    List<Chat> friends = await chatService.GetChatsByUserId(userId);
                    var userInfos = new List<string>();
                    foreach (var chat in friends)
                    {
                        User friendUser = await userService.GetUserById(chat.ReceiverId);
                        if (friendUser != null)
                        {
                            userInfos.Add($"{chat.UniqueId}|{chat.SenderId}|{chat.ReceiverId}|{friendUser.Username}");
                            Console.WriteLine("Friend: {0} {1} {2} {3}", friendUser.Username, friendUser.Id, chat.Id, chat.UniqueId);
                        }
                    }
                    return string.Join(',', userInfos);

                case "SEARCH_FRIENDS":
                    if (parts.Length < 3||!int.TryParse(parts[2], out int _userId))
                        return "Invalid request format. Expected: SEARCH_USER:userId";
                    string name = parts[1];
                    Console.WriteLine("Name: {0}", name);
                    List<User> friend = await userService.SearchFriends(name);
                    var userInfo = new List<string>();
                    foreach (var user in friend)
                    {
                        var isFriend = await friendsService.IsFriend(_userId, user.Id);
                        if (!isFriend && user.Id != _userId)
                        {
                            User friendUser = await userService.GetUserById(user.Id);

                            if (friendUser != null)
                            {
                                userInfo.Add($"{friendUser.Id}|{friendUser.Username}");
                                Console.WriteLine("Friend: {0} {1}", friendUser.Username, friendUser.Id);
                            }
                        }
                        else
                        {
                            return "NOT_FOUND";
                        }


                    }
                    return string.Join(',', userInfo);

                case "ADD_FRIEND":
                    if (parts.Length < 3 || !int.TryParse(parts[1], out int _userID) || !int.TryParse(parts[2], out int friendId))
                        return "Invalid request format. Expected: ADD_FRIEND:userId:friendId";

                    await friendsService.AddFriend(_userID, friendId);

                    await chatService.CreateChatAsync(_userID, friendId);
                    return "Friend added.";

                case "FETCH_MESSAGES":
                    if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid chatID))
                        return "Invalid request format. Expected: FETCH_MESSAGES:chatId";
                    List<Message> messages = await messageService.GetMessagesByChatAsync(chatID);
                    var messageInfos = new List<string>();
                    if (messages.Count==0)
                        return "NO_MESSAGES";
                    foreach (var messagee in messages)
                    {
                        User user = await userService.GetUserById(messagee.UserId);
                        if (user != null)
                        {
                            messageInfos.Add($"{user.Id}|{user.Username}|{messagee.Content}|{messagee.CreatedAt}");
                            Console.WriteLine("Message: {0} {1} {2}", user.Username, messagee.Id, messagee.Content);
                        }
                    }
                    return string.Join(',', messageInfos);

                default:
                    return "Unknown command.";
            }
        }
        catch (Exception ex)
        {
            // Log the exception (optional logging could be added here)
            return $"Error processing request: {ex.Message}";
        }
    }


}
