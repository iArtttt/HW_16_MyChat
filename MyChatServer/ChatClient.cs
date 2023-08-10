using MyChatLib;
using System.Net;
using System.Net.Sockets;


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
            Log("connected", MessageType.InformationMessege);

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
        
        internal void ChangeNameTo(string newNickName)
        {
            if (newNickName != null)
            {
                NickName = newNickName;
                this.NameChanged(NickName);                
            }
        }

        public Task Start() => _process = Task.Run(Menu.Process);

        internal void Log(string? message, MessageType messageType, ConsoleColor consoleColor = ConsoleColor.Gray)
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
            Log($"{NickName} Disconected", MessageType.InformationMessege, ConsoleColor.DarkRed);
            _reader.Dispose();
            _writer.Dispose();
            _stream.Close();
        }

        public event Action<EndPoint?, string?> MessageRecive;
        public event Action<ChatClient?, string?> MessagePrivateRecive;
    }
}