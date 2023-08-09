using MyChatLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;


namespace MyChatServer
{
    public class ChatClient : IDisposable
    {
        public string? NickName { get; private set; } = string.Empty;
        public int Age { get; private set; }
        internal string PrivateRoomName { get; set; } = string.Empty;

        public TcpClient TcpClient { get { return _tcpClient; } }

        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private Task? _process;
        private Menu Menu { get; set; }

        public ChatClient(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            Log("connected");

            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);
            MenuCreate();
        }
        private void MenuCreate()
        {
            try
            {
                Menu = Menu.DetectMenu<ClientMainMenu>(this);
            }
            catch { Dispose(); }
        }

        internal void MessageReciveUse(string? line) => MessageRecive?.Invoke(_tcpClient.Client.RemoteEndPoint, line);
        internal void MessagePrivateReciveUse(string? line) => MessagePrivateRecive?.Invoke(this, line);
        //internal void MessagePrivateReciveUse(string? line, EndPoint? receiver) => MessagePrivateRecive?.Invoke(_tcpClient.Client.RemoteEndPoint, receiver, line);
        
        internal void ChangeNameTo(string newNickName)
        {
            if (newNickName != null)
            {
                NickName = newNickName;
                this.NameChanged(NickName);                
            }
        }

        public Task Start() => _process = Task.Run(Menu.Process);
        
        internal void Log(string? message, ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            Console.ForegroundColor = consoleColor;
            if (NickName == string.Empty)
                Console.WriteLine($"[{_tcpClient.Client.RemoteEndPoint}]: {message}");
            else
                Console.WriteLine($"[{NickName}]: {message}");
            Console.ResetColor();
        }
        public void SendMessage(string message, MessageType messageType ,MessegeClientInfo clientInfo = default)
        {

            if(clientInfo == default)
                _writer.WriteLine($"{(byte)messageType}[{message}");
            else
                _writer.WriteLine($"{(byte)messageType}]{(byte)clientInfo}[{message}");

            _writer.Flush();
        }

        public void Dispose()
        {
            Log($"{NickName} Disconected", ConsoleColor.DarkRed);
            _reader.Dispose();
            _writer.Dispose();
            _stream.Close();
        }

        public event Action<EndPoint?, string?> MessageRecive;
        public event Action<ChatClient?, string?> MessagePrivateRecive;
    }
    class PrivateRoom
    {
        internal PrivateRoom(string roomName)
        {
            RoomName = roomName;
        }

        private int _people = 0;
        public string RoomName { get; }
        public bool IsAvailable { get; } = true;
        //private StreamReader reader;
        internal void Connect(ChatClient client)
        {
            try
            {
                if (_people >= 2) client.SendMessage($"Room ( {RoomName} ) is full", MessageType.PrivateChat, MessegeClientInfo.Allert);
                else
                {
                    _people++;
                    var reader = new StreamReader(client.TcpClient.GetStream());
                    client.PrivateRoomName = RoomName;
                    var line = string.Empty;
                    
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
                    reader.ReadLine();


                    client.Log($"Connect to private room {RoomName}", ConsoleColor.DarkBlue);
                    client.MessagePrivateReciveUse($"Connect to private room {RoomName}");
                    do
                    {
                        line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line))
                        {
                            client.Log(line, ConsoleColor.DarkBlue);
                            client.MessagePrivateReciveUse(line);
                        }

                    } while (!string.IsNullOrEmpty(line));

                    _people--;
                    client.Log($"Left {RoomName}", ConsoleColor.DarkBlue);
                    client.MessagePrivateReciveUse("Left Room");
                    client.PrivateRoomName = string.Empty;

                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                    reader.ReadLine();
                }
            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    internal static class Program
    {
        internal static List<PrivateRoom> PrivateRoomsList = new List<PrivateRoom>();

        private static List<ChatClient> _clients = new List<ChatClient>();
        private static Dictionary<EndPoint, string?> nickNames = new Dictionary<EndPoint, string?>();
        internal static List<ChatClient> Clients { get {  return _clients; } }
        private static async Task Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Any, 5002);
            listener.Start();

            Console.WriteLine($"MySercerChat started on: {listener.LocalEndpoint}");
            try
            {
                while (true)
                {
                    var client = new ChatClient(await listener.AcceptTcpClientAsync());
                    client.MessageRecive += Client_MessageReceive;
                    client.MessagePrivateRecive += Client_MessagePrivateReceive;
                    _clients.Add(client);
                    client.Start();
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        internal static void CreatePrivateRoom(this ChatClient client)
        {
            
            var reader = new StreamReader(client.TcpClient.GetStream());
            
            try
            {
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
                reader.ReadLine();

                var roomName = string.Empty;
                bool isFinish = false;

                do
                {
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.ClearClientConsole);

                    client.SendMessage($"Please Enter the new Private Room Name", MessageType.PrivateChat, MessegeClientInfo.Information);
                    roomName = reader.ReadLine();

                    if (roomName != string.Empty && !PrivateRoomsList.Any(rn => rn.RoomName == roomName))
                    {
                        PrivateRoomsList.Add(new PrivateRoom(roomName!));
                        client.SendMessage($"Room ( {roomName} ) was created", MessageType.PrivateChat, MessegeClientInfo.Succed);
                        client.Log($"Create PrivaeChatRoom: ( {roomName} )", ConsoleColor.Blue);
                        isFinish = true;
                    }
                    else
                    {
                        client.SendMessage($"Room ( {roomName} ) was NOT created", MessageType.PrivateChat, MessegeClientInfo.Allert);
                        client.SendMessage(null, MessageType.PrivateChat, MessegeClientInfo.Information);
                        
                        client.SendMessage($"Maybe this room has been already created", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage($"Or you write not correct", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage(null, MessageType.PrivateChat, MessegeClientInfo.Information);
                        
                        client.SendMessage($"Whould you like to try again?", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage($"Please write ( + ), ( Yes ) or ( Enter ) to try", MessageType.PrivateChat, MessegeClientInfo.Information);
                        var answer = reader.ReadLine();

                        if (answer == string.Empty || answer == "+" || answer.ToLower() == "yes")
                            isFinish = true;

                    }

                }while (!isFinish);

            }
            finally 
            {
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                reader.ReadLine();
            }
        }
        internal static void NameChanged(this ChatClient client, string  name)
        {
            nickNames.TryGetValue(client.TcpClient.Client.RemoteEndPoint!, out string? oldName);
            nickNames.TryAdd(client.TcpClient.Client.RemoteEndPoint!, name);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[{oldName ?? client.TcpClient.Client.RemoteEndPoint.ToString()}] change NickName to {name}");
            Console.ResetColor();

            client.SendMessage($"{name}", MessageType.InformationMessege, MessegeClientInfo.ChangeName);
            client.SendMessage($"You change your NickName to {name}", MessageType.InformationMessege, MessegeClientInfo.Succed);
        }
        private static void Client_MessageReceive(EndPoint? sender, string? text)
        {
            if (text != null)
            {
                if (nickNames.TryGetValue(sender, out var nickName))
                {
                    _clients.ForEach(c => c.SendMessage($"{nickName}]: {text}", MessageType.PublicChat));
                }
                else 
                    _clients.ForEach(c => c.SendMessage($"{sender}]: {text}", MessageType.PublicChat));
            }
        }
        private static void Client_MessagePrivateReceive(ChatClient? client, string? text)
        {
            if (text != null)
            {
                var resiver = _clients.FirstOrDefault(c => c.PrivateRoomName == client.PrivateRoomName, null);

                if (resiver != null)
                {
                    if (nickNames.TryGetValue(client.TcpClient.Client.RemoteEndPoint!, out var nickName))
                        resiver.SendMessage($"{nickName}]: {text}", MessageType.PrivateChat);
                    else
                        resiver.SendMessage($"{client}]: {text}", MessageType.PrivateChat);
                }

                //if (nickNames.TryGetValue(client.TcpClient.Client.RemoteEndPoint!, out var nickName))
                //    _clients.First(c => c.PrivateRoomName == client.PrivateRoomName)
                //        .SendMessage($"{nickName}]: {text}", MessageType.PrivateChat);                
                //else
                //    _clients.First(c => c.PrivateRoomName == client.PrivateRoomName)
                //        .SendMessage($"{client}]: {text}", MessageType.PrivateChat);
            }
        }
    }
}