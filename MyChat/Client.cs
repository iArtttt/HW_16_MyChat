using MyChatLib;
using MyChatServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyChat
{
    public class ServersSearch
    {
        public int Port { get; }
        public ServersSearch(int port = 6002)
        {
            Port = port;
        }

        public async Task SearchAsync(List<IPEndPoint> servers, CancellationToken cancellationToken = default)
        {
            UdpClient udpClient = new UdpClient();

            var msg = Encoding.UTF8.GetBytes("Who is Server?");
            var endPoint = new IPEndPoint(IPAddress.Broadcast, Port);

            await udpClient.SendAsync(msg, endPoint, cancellationToken);
        
            var recive = await udpClient.ReceiveAsync(cancellationToken);
            
            var reciveMessage = Encoding.UTF8.GetString(recive.Buffer);
            int port = -1;
            if (reciveMessage.StartsWith("I`m Server, port:"))
            {
                var ports = reciveMessage[(reciveMessage.IndexOf(":") + 1)..].Split(':');
                foreach (var p in ports)
                {
                    port = int.Parse(p.Trim());
                    if (!servers.Any(s => s.Port == port))
                    {
                        servers.Add(new IPEndPoint(recive.RemoteEndPoint.Address, port));
                        Console.WriteLine("I Find new server");
                    }
                }
            }

        }

    }
    internal class Client
    {
        private bool _isMenu = true;
        private bool _isInPublicChat = false;
        private string Name = string.Empty;

        private int _index = 0;
        private bool _isExit = false;
        internal List<IPEndPoint> _servers = new List<IPEndPoint>();


        public async Task Start()
        {
            var tasks = Task.Run(Process);

            await tasks;
        }

        public void Process()
        {
            ServersSearch serversSearch = new ServersSearch();
            while (!_isExit)
            {
                var searchingTask = Task.Run( () =>
                {
                     serversSearch.SearchAsync(_servers);
                });
                Console.Clear();
                if (_servers.Count > 0)
                {

                    Console.WriteLine($"Servers");

                    for (int i = 0; i < _servers.Count; i++)
                    {
                        if (i == _index)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"{_servers[i].Port} <--");
                            Console.ResetColor();
                        }
                        else
                        {

                            Console.WriteLine($"{_servers[i].Port}");
                        }
                    }

                    MoveEnter();
                }
                else
                {
                    Console.WriteLine("Sorry there is no servers, searching...");
                    Console.WriteLine("If you want to Exit press ( Q )");
                    var k = Console.ReadKey(true).Key;
                    
                    if (k == ConsoleKey.Q)
                        _isExit = true;
                }
            
            }
            _isExit = false;
        }
        private void MoveEnter()
        {
            try
            {

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.W:
                        _index = (_index - 1 < 0) ? _servers.Count - 1 : _index - 1;
                        break;
                    case ConsoleKey.UpArrow: goto case ConsoleKey.W;

                    case ConsoleKey.S:
                        _index = (_index + 1 > _servers.Count - 1) ? 0 : _index + 1;
                        break;
                    case ConsoleKey.DownArrow: goto case ConsoleKey.S;

                    case ConsoleKey.D:
                        try
                        {
                            ConnectTo(_servers[_index]);
                        }
                        catch
                        {
                            _index = 0;
                        }
                        break;
                    case ConsoleKey.RightArrow: goto case ConsoleKey.D;
                    case ConsoleKey.Enter: goto case ConsoleKey.D;

                    case ConsoleKey.A:
                        _isExit = true;
                        _index = 0;
                        break;
                    case ConsoleKey.Backspace: goto case ConsoleKey.A;
                    case ConsoleKey.LeftArrow: goto case ConsoleKey.A;
                    case ConsoleKey.Escape: goto case ConsoleKey.A;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private Task ConnectTo(IPEndPoint serverAddress)
        {
            using var tcpClient = new TcpClient(AddressFamily.InterNetwork);
            tcpClient.Connect(serverAddress);

            Console.WriteLine($"Client started on [{tcpClient.Client.LocalEndPoint}]");
            Console.WriteLine($"Connected to [{tcpClient.Client.RemoteEndPoint}]");


            var stream = tcpClient.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);


            var readerTask = Task.Run(() =>
            {
                string? line = null;
                do
                {
                    line = reader.ReadLine();
                    if (stream.CanRead)
                    {
                        try
                        {
                            var splited = line.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                            if (int.TryParse(splited[0], out int res))
                            {
                                switch (res)
                                {
                                    case (byte)MessageType.InformationMessege:

                                        Information(splited);

                                        break;

                                    case (byte)MessageType.PublicChat:

                                        PublicChat(splited, tcpClient);

                                        break;

                                    case (byte)MessageType.PrivateChat:

                                        PrivateChat(splited, tcpClient);

                                        break;

                                    case (byte)MessageType.ClearClientConsole:

                                        Console.Clear();

                                        break;

                                }
                            }
                        }
                        catch { }

                    }

                } while (!reader.EndOfStream);
            });


            var writerTask = Task.Run(() =>
            {
                do
                {
                    try
                    {
                        if (_isMenu)
                        {
                            var line = Convert.ToInt32(Console.ReadKey().Key);
                            writer.WriteLine(line);
                            writer.Flush();
                        }
                        else
                        {
                            var line = Console.ReadLine();
                            writer.WriteLine(line);
                            writer.Flush();
                        }
                    }
                    catch { stream.Close(); }
                }
                while (stream.CanWrite);
            });

            Task.WaitAll(readerTask, writerTask);
            Console.Clear();
            Console.WriteLine("Disconected");
            Console.ReadKey();
            return Task.CompletedTask;
        }

        private void Information(string[] splited)
        {
            if (int.TryParse(splited[1], out int infoType))
            {
                switch (infoType)
                {
                    case (byte)MessegeClientInfo.Allert:

                        Print(splited, 2, ConsoleColor.Red);

                        break;

                    case (byte)MessegeClientInfo.Succed:

                        Print(splited, 2, ConsoleColor.Green);

                        break;

                    case (byte)MessegeClientInfo.Information:

                        Print(splited, 2, ConsoleColor.Yellow);
                        break;


                    case (byte)MessegeClientInfo.MenuTrue:

                        _isMenu = true;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Please press ( Enter ) to continue...");
                        }
                        break;

                    case (byte)MessegeClientInfo.MenuFalse:

                        _isMenu = false;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Please press ( Any Button ) to continue...");
                        }

                        break;
                    case (byte)MessegeClientInfo.PublicChatTrue:

                        _isInPublicChat = true;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);

                        break;

                    case (byte)MessegeClientInfo.PublicChatFalse:

                        _isInPublicChat = false;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);
                        break;

                    case (byte)MessegeClientInfo.ChangeName:

                        Name = (splited.Skip(3).FirstOrDefault() != null)
                            ? splited.Skip(2).Aggregate((f, c) => f + " " + c)
                            : splited[2];

                        break;
                    default:

                        Print(splited, 2, ConsoleColor.White);

                        break;
                }
            }
            else
            {
                Print(splited);
            }
        }
        private void PublicChat(string?[] splited, TcpClient tcpClient)
        {

            if (Name != string.Empty)
            {
                if (splited[1].ToString() != $"{Name}" && _isInPublicChat)
                    Print(splited);
            }
            else if (splited[1] != tcpClient.Client.LocalEndPoint.ToString() && _isInPublicChat)
                Print(splited);
        }
        private void PrivateChat(string?[] splited, TcpClient tcpClient)
        {

            if (int.TryParse(splited[1], out int infoType))
            {
                switch (infoType)
                {
                    case (byte)MessegeClientInfo.Allert:

                        Print(splited, 2, ConsoleColor.DarkRed);

                        break;


                    case (byte)MessegeClientInfo.Succed:

                        Print(splited, 2, ConsoleColor.DarkGreen);

                        break;

                    case (byte)MessegeClientInfo.Information:

                        Print(splited, 2, ConsoleColor.DarkYellow);

                        break;

                    default:

                        Print(splited, ConsoleColor.Blue);

                        break;
                }
            }
            else
            {
                Print(splited, ConsoleColor.DarkBlue);
            }
        }

        private void Print(string?[] splited, ConsoleColor consoleColor) => Print(splited, 1, consoleColor);
        private void Print(string?[] splited, int skipelements = 1) => Print(splited, skipelements, ConsoleColor.Gray);
        private void Print(string?[] splited, int skipelements, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;

            if (splited.Skip(skipelements + 1).FirstOrDefault() != null)
                Console.WriteLine(splited.Skip(skipelements).Aggregate((f, c) => f + " " + c));
            else if (splited.Skip(skipelements).FirstOrDefault() != null)
                Console.WriteLine(splited[skipelements]);
            else
                Console.WriteLine();

            Console.ResetColor();
        }
    }
}
