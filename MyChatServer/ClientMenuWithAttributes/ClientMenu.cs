using MyChatLib;

namespace MyChatServer
{
    internal class ClientMainMenu
    {
        [MenuSubmenu("LogIn", 1)]
        public ClientSubMenu SubMenu { get; set; }

        [MenuSubmenu("Registration", 2)]
        public ClientRegMenu RegMenu { get; set; }

        [MenuAction("Disconect", int.MaxValue)]
        public void Disconect(ChatClient client) 
        {
            client.Dispose();
            return;
        }

    }
    internal class ClientRegMenu
    {
        [MenuAction("PropOne", 1)]
        public void PropOne(ChatClient client)
        {
            
        }
        [MenuAction("PropTwo", 2)]
        public void PropTwo(ChatClient client)
        {

        }
    }
    internal class ClientSubMenu
    {
        [MenuAction("PublicChat", 1)]
        public void PublicChat(ChatClient client)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Clear);
            var reader = new StreamReader(client.TcpClient.GetStream());
            client.SendMessage("Wait a few second connecting...", MessageType.InformationMessege, MessegeClientInfo.Information);
            Thread.Sleep(2000);
            client.SendMessage("You are in public chat now", MessageType.InformationMessege, MessegeClientInfo.Information);
            
            string? line = null;
            do
            {
                line = reader.ReadLine();


                client.Log(line);
                client.MessageReciveUse(line);

            } while (!string.IsNullOrEmpty(line));
            
            client.SendMessage("Wait a few second disconnecting...", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
            Thread.Sleep(2000);
        }

        [MenuAction("PrivateChat", 2)]
        public void PrivateChat(ChatClient client)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);

            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
        }
        [MenuAction("ChangeName", 3)]
        public void ChangeName(ChatClient client)
        {
            client.ChangeName();
        }
    }
}