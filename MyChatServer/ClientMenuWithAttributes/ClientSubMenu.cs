using MyChatLib;

namespace MyChatServer
{
    internal class ClientSubMenu
    {
        [MenuAction("PublicChat", 1, "Connect to public chat")]
        public void PublicChat(ChatClient client)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            
            var reader = new StreamReader(client.TcpClient.GetStream());

            client.SendMessage("Press any button", MessageType.InformationMessege, MessegeClientInfo.Information);
            reader.ReadLine();
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            
            client.SendMessage("Wait a few second connecting...", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            Thread.Sleep(200);
            
            client.SendMessage("If you want to liave public chat press Enter", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("You are in public chat now", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("Change Public Chat Access", MessageType.InformationMessege, MessegeClientInfo.Information);
            
            client.Log($"Conected to Public Chat", ConsoleColor.DarkGreen);

            string? line = null;
            do
            {
                line = reader.ReadLine();


                client.Log(line);
                client.MessageReciveUse(line);

            } while (!string.IsNullOrEmpty(line));
            
            client.SendMessage("Press any button disconnecting...", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("Change Public Chat Access", MessageType.InformationMessege, MessegeClientInfo.Information);

            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
            reader.ReadLine();

            client.Log($"Disconected from Public Chat", ConsoleColor.DarkRed);
        }


        [MenuAction("PrivateChat", 2, "Connect to private chat")]
        public void PrivateChatSelect(ChatClient client)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            client.SendMessage("Press any button to continue", MessageType.InformationMessege, MessegeClientInfo.Information);

            var reader = new StreamReader(client.TcpClient.GetStream());

            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);


            SelectPerson(client, reader);


            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
            client.SendMessage("Press any button to continue", MessageType.InformationMessege, MessegeClientInfo.Information);
            reader.ReadLine();
        }

        private int _index = 0;
        private bool _isExit = false;
        private List<ChatClient> _clientsList;
        private void SelectPerson(ChatClient client, StreamReader reader)
        {
            _clientsList = Program.Clients.Where(c => c.TcpClient != client.TcpClient && c.TcpClient.Connected).ToList();
            if(_clientsList.Count > 0)
            {

                while (!_isExit && client.TcpClient.Connected)
                {
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
                    client.SendMessage("Choose you want to chat with", MessageType.InformationMessege, MessegeClientInfo.Information);

                    client.SendMessage($"----( Users )----", MessageType.InformationMessege, MessegeClientInfo.Information);
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);

                    for (int i = 0; i < _clientsList.Count; i++)
                    {
                        if (i == _index)
                            client.SendMessage($"{_clientsList[i].NickName} <--", MessageType.Menu);
                        else
                            client.SendMessage($"{_clientsList[i].NickName}", MessageType.Menu);
                    }

                    MoveEnter(client, reader);
                }
                _isExit = false;
            }
            else
            {
                client.SendMessage($"Sorry there is no users exept you on this server", MessageType.InformationMessege, MessegeClientInfo.Allert);
            }
            reader.ReadLine();
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
        }
        private void MoveEnter(ChatClient client, StreamReader reader)
        {
            var key = (ConsoleKey)Convert.ToInt32(reader.ReadLine());

            switch (key)
            {
                case ConsoleKey.W:
                    _index = (_index - 1 < 0) ? _clientsList.Count - 1 : _index - 1;
                    break;
                case ConsoleKey.UpArrow: goto case ConsoleKey.W;

                case ConsoleKey.S:
                    _index = (_index + 1 > _clientsList.Count - 1) ? 0 : _index + 1;
                    break;
                case ConsoleKey.DownArrow: goto case ConsoleKey.S;

                case ConsoleKey.D:
                    try
                    {
                        client.SendMessage($"Waiting for answer...", MessageType.InformationMessege, MessegeClientInfo.Information);

                        var client2 = _clientsList[_index];
                        var stream2 = client2.TcpClient.GetStream();
                        var reader2 = new StreamReader(stream2);
                        //var writer2 = new StreamWriter(stream2);


                        if (client.NickName != string.Empty)
                        {
                            client2.SendMessage($"Whould you like to have private chat with [{client.NickName}", MessageType.PrivateChat);

                        }
                        else
                        {
                            client2.SendMessage(
                                $"Whould you like to have private chat with [{client.TcpClient.Client.RemoteEndPoint}", 
                                MessageType.PrivateChat, 
                                MessegeClientInfo.Information);
                        }

                        client2.SendMessage(null, MessageType.Menu, MessegeClientInfo.MenuTrue);
                        client2.SendMessage(null, MessageType.PrivateChat, MessegeClientInfo.Allert);
                        client2.SendMessage("Press any button to continue and then answer", MessageType.InformationMessege, MessegeClientInfo.Information);
                        reader2.ReadLine();
                        client2.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
                        
                        client2.SendMessage($"Please Write (Yes, +) if you would like", MessageType.InformationMessege, MessegeClientInfo.Information);
                        var tempLine = reader2.ReadLine().ToLower();

                        if (tempLine == "yes" || tempLine == "+")
                        {
                            client2.SendMessage($"{client.TcpClient.Client.RemoteEndPoint}", MessageType.PrivateChat, MessegeClientInfo.Succed);
                            client.SendMessage($"{client2.TcpClient.Client.RemoteEndPoint}", MessageType.PrivateChat, MessegeClientInfo.Succed);
                            client.Log($"{client.TcpClient.Client.RemoteEndPoint} and {client2.TcpClient.Client.RemoteEndPoint} star private chat", ConsoleColor.Blue);
                            PrivateChat(client, client2);
                        }
                        else
                        {

                        }



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
            }
        }
        private void PrivateChat(ChatClient client, ChatClient client2)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            client2.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            client2.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            client.SendMessage("Press any button tof continue", MessageType.InformationMessege, MessegeClientInfo.Information);
            client2.SendMessage("Press any button tos continue", MessageType.InformationMessege, MessegeClientInfo.Information);

            var reader = new StreamReader(client.TcpClient.GetStream());
            var reader2 = new StreamReader(client.TcpClient.GetStream());
            reader.ReadLine();
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);

            string? line = null;
            do
            {
                line = reader.ReadLine();

                if(!string.IsNullOrEmpty(line))
                {



                    client.Log(line, ConsoleColor.Blue);
                    client.MessagePrivateReciveUse(line, null);
                }


            } while (!string.IsNullOrEmpty(line));


            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
            client.SendMessage("Press any button to continue", MessageType.InformationMessege, MessegeClientInfo.Information);
            reader.ReadLine();
        }


        [MenuAction("ChangeName", 3, "Here you can change your NikName, other people will see it")]
        public void ChangeName(ChatClient client)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            client.SendMessage("Press any button", MessageType.InformationMessege, MessegeClientInfo.Information);
            
            bool isChanged = false;
            string? newNickName;
            var reader = new StreamReader(client.TcpClient.GetStream());
            reader.ReadLine();
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            
            do
            {
                client.SendMessage("Please Enter your new NickName: ", MessageType.InformationMessege, MessegeClientInfo.Information);

                newNickName = reader.ReadLine();
                if (newNickName != null && newNickName != string.Empty)
                {
                    client.SendMessage($"Would you like to get NickName {newNickName}?", MessageType.InformationMessege, MessegeClientInfo.Information);
                    client.SendMessage($"Please Write (Yes, +) if you would like", MessageType.InformationMessege, MessegeClientInfo.Information);
                    var tempLine = reader.ReadLine().ToLower();
                    if (tempLine == "yes" || tempLine == "+" || tempLine == string.Empty )
                        isChanged = true;
                }

            } while (!isChanged);

            client.ChangeNameTo(newNickName);
            
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
            client.SendMessage("Press any button to continue", MessageType.InformationMessege, MessegeClientInfo.Information);
            reader.ReadLine();
        }
    }
}