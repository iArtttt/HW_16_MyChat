using MyChatLib;


namespace MyChatServer
{
    internal class PrivateRoom
    {
        internal PrivateRoom(string roomName)
        {
            RoomName = roomName;
        }

        private int _people = 0;
        public string RoomName { get; }
        public bool IsAvailable { get; private set; } = true;
        internal void Connect(ChatClient client)
        {
            try
            {
                var reader = new StreamReader(client.TcpClient.GetStream());
                if (!IsAvailable)
                {
                    client.SendMessage($"Room ( {RoomName} ) is full", MessageType.PrivateChat, MessegeClientInfo.Allert);
                    reader.ReadLine();
                }
                else
                {
                    _people++;

                    if (_people == 2) { IsAvailable = false; }

                    
                    client.PrivateRoomName = RoomName;
                    var line = string.Empty;

                    client.SendMessage(null, MessageType.ClearClientConsole);
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
                    reader.ReadLine();

                    client.SendMessage(null, MessageType.ClearClientConsole);
                    client.SendMessage($"You connect to private chat room {RoomName}", MessageType.PrivateChat);
                    client.SendMessage(null, MessageType.PrivateChat);


                    client.Log($"Connect to private room {RoomName}", MessageType.InformationMessege, ConsoleColor.DarkBlue);
                    client.MessagePrivateReciveUse($"Connect to private room");
                    do
                    {
                        line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line))
                        {
                            client.Log(line, MessageType.PrivateChat, ConsoleColor.DarkBlue);
                            client.MessagePrivateReciveUse(line);
                        }

                    } while (!string.IsNullOrEmpty(line));

                    _people--;
                    client.Log($"Left {RoomName}", MessageType.InformationMessege, ConsoleColor.DarkBlue);
                    client.MessagePrivateReciveUse("Left Room");
                    client.PrivateRoomName = string.Empty;

                    client.SendMessage($"You left private chat room {RoomName}", MessageType.PrivateChat);
                    client.SendMessage(null, MessageType.PrivateChat);
                    client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                    
                    reader.ReadLine();
                }
            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (_people < 2) { IsAvailable = true; }
            }
        }
    }
}