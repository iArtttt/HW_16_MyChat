﻿using MyChatLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyChatServer
{
    class UDPClientSearch
    {
        private readonly int _port;
        public readonly UdpClient udpFromClient;
        public UDPClientSearch(int port = 6002)
        {
            bool isComplete = false;
            do
            {
                try
                {
                    _port = port;
                    udpFromClient = new UdpClient(_port);
                    isComplete = true;
                }
                catch { port++; }
            }while (!isComplete);
        }
        public async Task Send(byte[] mes, IPEndPoint iPEndPoint, CancellationToken cancellationToken = default)
        {
            await udpFromClient.SendAsync(mes, iPEndPoint, cancellationToken);
        }
        public async Task Listen(CancellationToken cancellationToken = default)
        {
            
            do
            {

                var recivedData = await udpFromClient.ReceiveAsync(cancellationToken);
                var messege = Encoding.UTF8.GetString(recivedData.Buffer);

                if (messege == "Who is Server?")
                {
                    var ld = string.Join(':', "I`m Server, port:" + string.Join(':',Server.ServersList.Select(p => p.Port).ToArray()));
                    var mes = Encoding.UTF8.GetBytes(ld);
                    await udpFromClient.SendAsync(mes, recivedData.RemoteEndPoint, cancellationToken);

                    Server.AllClientsList.Add(recivedData.RemoteEndPoint);
                }
                
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
    class UDPServerSearch
    {
        private readonly int _port;
        private readonly UdpClient resUDP;
        public UDPClientSearch ClientSearch { get; }
        public UDPServerSearch(UDPClientSearch clientSearch, int port = 5002)
        {
            bool isComplete = false;
            do
            {
                try
                {
                    _port = port;
                    resUDP = new UdpClient(_port);
                    isComplete = true;
                }
                catch { port++; }
            } while (!isComplete);
            ClientSearch = clientSearch;
        }


        public async Task Search(CancellationToken cancellationToken = default)
        {
            var udp = new UdpClient();
            
            var msg = Encoding.UTF8.GetBytes($"I`m new Server, port: {Server.Port}");
            var endPoint = new IPEndPoint(IPAddress.Broadcast, 5002);
            await udp.SendAsync(msg, endPoint, cancellationToken);
            
            var resendTask = Task.Run(async () => 
            {
                do
                {
                    var get = await udp.ReceiveAsync();
                    var getMess = Encoding.UTF8.GetString(get.Buffer);

                    var portStr = getMess[(getMess.IndexOf(":") + 1)..].Trim();
                    var port = int.Parse(portStr);
                    if (!Server.ServersList.Any(s => s.Port == port))
                    {
                        Console.WriteLine($"Find new server, port: {port}");
                        Server.ServersList.Add(new IPEndPoint(get.RemoteEndPoint.Address, port));
                    }


                } while (!cancellationToken.IsCancellationRequested);
            });

            do
            {
                var recivedData = await resUDP.ReceiveAsync(cancellationToken);
                var messege = Encoding.UTF8.GetString(recivedData.Buffer);

                if (messege.StartsWith("I`m new Server, port:"))
                {
                    var ports = messege[(messege.IndexOf(":") + 1)..].Split(':');

                    foreach( var p in ports)
                    {
                        var port = int.Parse(p);
                        if (!Server.ServersList.Any(s => s.Port == port))
                        {
                            Console.WriteLine($"Find new server, port: {port}");

                            var messegeToSend = string.Join(':', "I`m new Server, port:" + string.Join(':', Server.ServersList.Select(p => p.Port).ToArray()));
                            var bytesToSend = Encoding.UTF8.GetBytes(messegeToSend);

                            await udp.SendAsync(bytesToSend, recivedData.RemoteEndPoint, cancellationToken);

                            Server.ServersList.Add(new IPEndPoint(recivedData.RemoteEndPoint.Address, port));

                            for (int i = 0; i < Server.AllClientsList.Count; i++)
                            {
                                var mess = Encoding.UTF8.GetBytes($"I`m Server, port: {port}");
                                await ClientSearch.Send(bytesToSend, Server.AllClientsList[i], cancellationToken);
                            }
                        }
                    }
                }
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
    internal static class Server
    {
        public static string ServerName { get; }
        public static int Port { get; private set; }
        internal static List<ChatClient> Clients { get { return _clientsList; } }

        internal static List<string> PublicChatMesseges = new List<string>();
        internal static List<PrivateRoom> PrivateRoomsList = new List<PrivateRoom>();
        internal static List<IPEndPoint> ServersList = new List<IPEndPoint>();
        internal static List<IPEndPoint> AllClientsList = new List<IPEndPoint>();

        private static List<ChatClient> _clientsList = new List<ChatClient>();
        private static Dictionary<EndPoint, string?> nickNames = new Dictionary<EndPoint, string?>();
        
        private static TcpListener Init()
        {
            do
            {
                try
                {
                    Port = new Random().Next(IPEndPoint.MaxPort);
                    return new TcpListener(IPAddress.Any, Port);
                }
                catch 
                {
                    
                }
            }while (true);

        }
        public static async Task Listen(CancellationToken cancellationToken = default)
        {
            var udp = new UdpClient(Port);

            do
            {
                var recivedData = await udp.ReceiveAsync(cancellationToken);
                var messege = Encoding.UTF8.GetString(recivedData.Buffer);

                if (messege == "Who is Server?")
                {
                    messege = $"I`m Server, port: {Port}";

                    await udp.SendAsync(Encoding.UTF8.GetBytes(messege), recivedData.RemoteEndPoint, cancellationToken);
                }



            } while (!cancellationToken.IsCancellationRequested);


        }
        public static async Task Start()
        {
            
            var listener = Init();
            ServersList.Add(new IPEndPoint(IPAddress.Any, Port));
            listener.Start();
            Console.WriteLine($"MySercerChat started on: {listener.LocalEndpoint}");

            UDPClientSearch udpClients = new ();
            udpClients.Listen();
            Console.WriteLine($"Listening Users");

            UDPServerSearch udpServer = new UDPServerSearch(udpClients);
            udpServer.Search();
            Console.WriteLine($"Searching Servers");

            try
            {
                while (true)
                {
                    var client = new ChatClient(await listener.AcceptTcpClientAsync());
                    client.MessageRecive += Client_MessageReceive;
                    client.MessagePrivateRecive += Client_MessagePrivateReceive;
                    _clientsList.Add(client);
                    client.Start();
                }
            }
            finally
            {
                listener.Stop();
            }
        }


        internal static void CreatePrivateRoom(ChatClient client)
        {

            var reader = new StreamReader(client.TcpClient.GetStream());

            try
            {
                client.SendMessage(MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
                reader.ReadLine();

                var roomName = string.Empty;
                bool isFinish = false;

                do
                {
                    client.SendMessage(MessageType.ClearClientConsole);

                    client.SendMessage($"Please Enter the new Private Room Name", MessageType.PrivateChat, MessegeClientInfo.Information);
                    roomName = reader.ReadLine();

                    if (roomName != string.Empty && !PrivateRoomsList.Any(rn => rn.RoomName == roomName))
                    {


                        client.SendMessage($"Would you like to create room ( {roomName} )?", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage($"Please write ( + ), ( Yes ) or ( Enter ) to try", MessageType.PrivateChat, MessegeClientInfo.Information);
                        var answer = reader.ReadLine();

                        if (answer == string.Empty || answer == "+" || answer.ToLower() == "yes")
                        {
                            isFinish = true;
                            PrivateRoomsList.Add(new PrivateRoom(roomName!));
                            client.SendMessage($"Room ( {roomName} ) was created", MessageType.PrivateChat, MessegeClientInfo.Succed);
                            client.Log($"Create PrivaeChatRoom: ( {roomName} )", MessageType.InformationMessege, ConsoleColor.Blue);
                        }
                    }
                    else
                    {
                        client.SendMessage($"Room ( {roomName} ) was NOT created", MessageType.PrivateChat, MessegeClientInfo.Allert);
                        client.SendMessage(MessageType.PrivateChat, MessegeClientInfo.Information);

                        client.SendMessage($"Maybe this room has been already created", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage($"Or you write not correct", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage(MessageType.PrivateChat, MessegeClientInfo.Information);

                        client.SendMessage($"Whould you like to try again?", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage($"Please write ( + ), ( Yes ) or ( Enter ) to try", MessageType.PrivateChat, MessegeClientInfo.Information);
                        var answer = reader.ReadLine();

                        if (answer != string.Empty || answer != "+" || answer.ToLower() != "yes")
                            isFinish = true;

                    }

                } while (!isFinish);

            }
            finally
            {
                client.SendMessage(MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                reader.ReadLine();
            }
        }
        internal static void NameChanged(ChatClient client, string name)
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
                    PublicChatMesseges.Add($"{nickName} {DateTime.Now.Hour}:{DateTime.Now.Minute}: {text}");
                    _clientsList.ForEach(c => c.SendMessage($"{nickName}]: {text}", MessageType.PublicChat));
                }
                else
                {
                    PublicChatMesseges.Add($"{sender} {DateTime.Now.Hour}:{DateTime.Now.Minute}: {text}");
                    _clientsList.ForEach(c => c.SendMessage($"{sender}]: {text}", MessageType.PublicChat));
                }
            }
        }
        private static void Client_MessagePrivateReceive(ChatClient? client, string? text)
        {
            if (text != null)
            {
                var resiver = _clientsList.FirstOrDefault(c => c.PrivateRoomName == client.PrivateRoomName &&
                                c.TcpClient.Client.RemoteEndPoint != client.TcpClient.Client.RemoteEndPoint,
                                null);

                if (resiver != null)
                {
                    if (nickNames.TryGetValue(client.TcpClient.Client.RemoteEndPoint!, out var nickName))
                        resiver.SendMessage($"{nickName}]: {text}", MessageType.PrivateChat);
                    else
                        resiver.SendMessage($"{client}]: {text}", MessageType.PrivateChat);
                }
            }
        }
    }
}
