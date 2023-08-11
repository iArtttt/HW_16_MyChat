﻿using MyChatLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyChatServer
{
    internal static class Server
    {
        public static string ServerName { get; }

        private static Dictionary<EndPoint, string?> nickNames = new Dictionary<EndPoint, string?>();
        private static List<ChatClient> _clientsList = new List<ChatClient>();
        
        internal static List<PrivateRoom> PrivateRoomsList = new List<PrivateRoom>();
        internal static List<ChatClient> Clients { get { return _clientsList; } }

        internal static List<string> PublicChatMesseges = new List<string>();

        public static async Task Start()
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

                        if (answer == string.Empty || answer == "+" || answer.ToLower() == "yes")
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
