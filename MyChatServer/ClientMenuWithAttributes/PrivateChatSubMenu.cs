using MyChatLib;

namespace MyChatServer
{
    internal class PrivateChatSubMenu
    {
        [MenuAction("PrivateChat", 1, "Connect to private chat room")]
        public void PrivateChatSelect(ChatClient client)
        {
            client.SendMessage(null, MessageType.ClearClientConsole);
            var reader = new StreamReader(client.TcpClient.GetStream());

            if (Program.PrivateRoomsList.Count > 0)
            {
                _roomsAvailable = Program.PrivateRoomsList.Where(r => r.IsAvailable).ToList();

                if (_roomsAvailable.Count > 0)
                {


                    while (!_isExit && client.TcpClient.Connected)
                    {

                        client.SendMessage(null, MessageType.ClearClientConsole);
                        client.SendMessage("Choose wich Room you want to chat:", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage(null, MessageType.PrivateChat, MessegeClientInfo.Information);

                        client.SendMessage($"-----( Rooms Available )-----", MessageType.PrivateChat, MessegeClientInfo.Information);
                        client.SendMessage(null, MessageType.PrivateChat, MessegeClientInfo.Information);

                        for (int i = 0; i < Program.PrivateRoomsList.Count; i++)
                        {
                            if (i == _index)
                            {
                                client.SendMessage($"{_roomsAvailable[i].RoomName} <--", MessageType.PrivateChat, MessegeClientInfo.Succed);
                            }
                            else
                            {
                                client.SendMessage($"{_roomsAvailable[i].RoomName}", MessageType.PrivateChat);
                            }
                        }

                        MoveEnter(client, reader);
                    }
                    _isExit = false;
                }
                else
                {
                    client.SendMessage("There is no any free Privates Room", MessageType.PrivateChat, MessegeClientInfo.Allert);
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                    reader.ReadLine();
                } 
            }
            else
            {
                client.SendMessage("There is no existing Privates Room", MessageType.PrivateChat, MessegeClientInfo.Allert);
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                reader.ReadLine();
            }

            
            //client.SendMessage(null, MessageType.PrivateChat, MessegeClientInfo.ClearClientConsole);

        }

        private int _index = 0;
        private bool _isExit = false;
        private List<PrivateRoom> _roomsAvailable;
        
        private void MoveEnter(ChatClient client, StreamReader reader)
        {
            var key = (ConsoleKey)Convert.ToInt32(reader.ReadLine());

            switch (key)
            {
                case ConsoleKey.W:
                    _index = (_index - 1 < 0) ? _roomsAvailable.Count - 1 : _index - 1;
                    break;
                case ConsoleKey.UpArrow: goto case ConsoleKey.W;

                case ConsoleKey.S:
                    _index = (_index + 1 > _roomsAvailable.Count - 1) ? 0 : _index + 1;
                    break;
                case ConsoleKey.DownArrow: goto case ConsoleKey.S;

                case ConsoleKey.D:
                    try
                    {
                        _roomsAvailable[_index].Connect(client);
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

        [MenuAction("Create Room", 2, "Here you can create new Private chat room")]
        public void PrivateRoomCreate(ChatClient client)
        {
            client.CreatePrivateRoom();
        }
    }
}