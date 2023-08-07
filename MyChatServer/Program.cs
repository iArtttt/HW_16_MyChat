using MyChatLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace MyChatServer
{
    public class ChatClient : IDisposable
    {
        public string? NickName { get; private set; } = string.Empty;
        public int Age { get; private set; }
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
        internal void MessageReciveUse(string? line)
        {
            MessageRecive?.Invoke(_tcpClient.Client.RemoteEndPoint, line);
        }
        internal void ChangeName()
        {
            SendMessage("Please Enter your NickName: ", MessageType.InformationMessege, MessegeClientInfo.Information);
            _writer.Write("Please Enter your NickName: ");
            var newNickName = _reader.ReadLine();
            if (newNickName != null)
            {
                NickName = newNickName;
                this.NameChanged(NickName);                
            }
        }

        public Task Start()
        {
            return _process = Task.Run(() =>
            {
                Menu.Process();
                //string? line = null;
                //do
                //{
                //    //line = _reader.ReadLine();
                //    if (line != string.Empty && int.TryParse(line[0].ToString(),out int result) && result == ((byte)MessageType.System))
                //    {
                //        //_writer.Flush();
                //        //SendMessage("Please Enter your NickName: ", ConsoleColor.DarkYellow);
                //        ////_writer.Write("Please Enter your NickName: ");
                //        //var nickName = _reader.ReadLine();
                //        //ChangeName(nickName);
                //        Menu.Process();
                //        Console.WriteLine("You GOT IT");
                //    }
                //    else
                //    {
                //        Log(line);
                //        MessageRecive?.Invoke(_tcpClient.Client.RemoteEndPoint, line);
                //    }



                //} while (/*!string.IsNullOrEmpty(line)*/true);
            });
        }
        internal void Log(string? message, ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($"[{_tcpClient.Client.RemoteEndPoint}]: {message}");
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
            _stream.Close();
            //_reader.Dispose();
            //_writer.Dispose();
        }

        public event Action<EndPoint?, string?> MessageRecive;
    }


    internal static class Program
    {
        private static List<ChatClient> clients = new List<ChatClient>();
        private static Dictionary<EndPoint, string?> nickNames = new Dictionary<EndPoint, string?>();
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
                    client.MessageRecive += Client_MessageRecive;
                    clients.Add(client);
                    client.Start();
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        internal static void NameChanged(this ChatClient client, string  name)
        {
            nickNames.TryGetValue(client.TcpClient.Client.RemoteEndPoint!, out string? oldName);
            nickNames.TryAdd(client.TcpClient.Client.RemoteEndPoint!, name);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{oldName ?? client.TcpClient.Client.RemoteEndPoint.ToString()} change NickName to {name}");
            Console.ResetColor();
            client.SendMessage($"You change your NickName to {name}", MessageType.InformationMessege, MessegeClientInfo.Succed);
        }
        private static void Client_MessageRecive(EndPoint? sender, string? obj)
        {
            if (obj != null)
            {
                if (nickNames.TryGetValue(sender, out var nickName))
                {
                    clients.ForEach(c => c.SendMessage($"{nickName}]: {obj}", MessageType.PublicChat));
                }
                else 
                    clients.ForEach(c => c.SendMessage($"{sender}]: {obj}", MessageType.PublicChat));
            }
        }
    }
}