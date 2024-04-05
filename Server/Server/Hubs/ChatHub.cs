using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Server.Model;

namespace Server.Hubs;

public class ChatHub : Hub
{
    private readonly string _botUser;
    private readonly IDictionary<string, UserData> _connections;

    public ChatHub(IDictionary<string, UserData> connections)
    {
        _connections = connections;
        _botUser = "Бот";
    }


    public async Task JoinToRoom(UserData userData)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userData.Room);

        _connections[Context.ConnectionId] = userData;

        await Clients.Group(userData.Room).SendAsync("ReceiveMessage", _botUser, $"{userData.User} присоединился в чат");

        await SendConnectedUsers(userData.Room);
    }

    public async Task SendMessage(string message)
    {
        if(_connections.TryGetValue(Context.ConnectionId, out UserData Login))
        {
            var time = GetTimeString(DateTime.Now);
            await Clients.Group(Login.Room).SendAsync("ReceiveMessage", 
                Login.User, 
                message,
                time);
            LogMessage(Login, message, time);
        }
    }

    public Task SendConnectedUsers(string room)
    {
        var users = _connections.Values
            .Where(d => d.Room == room)
            .Select(d => d.User);

        return Clients.Group(room).SendAsync("UsersInRoom", users);
    }
    
    public override Task OnDisconnectedAsync(Exception exception)
    {
        if(_connections.TryGetValue(Context.ConnectionId, out UserData Login))
        {
            _connections.Remove(Context.ConnectionId);
            Clients.Group(Login.Room).SendAsync("ReceiveMessage", _botUser, $"{Login.User} покинул чат");
        }

        return base.OnDisconnectedAsync(exception);
    }

    private string GetTimeString(DateTime time)
    {
        string day = (time.Day + 7 < 10 ? "0" : "") + (time.Day + 7);
        string month = (time.Month - 3 < 10 ? "0" : "") + (time.Month - 3);
        string hour = (time.Hour + 13 < 10 ? "0" : "") + (time.Hour + 13);
        string minute = (time.Minute < 10 ? "0" : "") + time.Minute;
        string second = (time.Second < 10 ? "0" : "") + time.Second;

        return $"{day}/{month} {hour}:{minute}:{second}";
    }

    private void LogMessage(UserData userData, string message, string time)
    {
        using var writer = new StreamWriter("log.txt", append:true);
        
        writer.WriteLine($"{time} - {userData.Room} - {userData.User} - {message}");
    }
}