using MyChatLib;
using System.Reflection.PortableExecutable;

namespace MyChatServer
{
    internal class ClientSubMenu
    {
        [MenuAction("PublicChat", 1, "Connect to public chat")]
        public void PublicChat(ChatClient client)
        {
            var reader = new StreamReader(client.TcpClient.GetStream());
            try
            {

                client.SendMessage(null, MessageType.ClearClientConsole);
                
                client.SendMessage("Wait a few seconds connecting...", MessageType.InformationMessege, MessegeClientInfo.PublicChatTrue);
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);

                reader.ReadLine();
            
                client.SendMessage(null, MessageType.ClearClientConsole);
                client.SendMessage("If you want to leave public chat press ( Enter )", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("You are in public chat now", MessageType.InformationMessege, MessegeClientInfo.Succed);
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
                
                foreach (var item in Program.PublicChatMesseges)
                {
                    client.SendMessage($"{item}", MessageType.InformationMessege);
                }

                client.Log($"Conected to Public Chat", MessageType.InformationMessege, ConsoleColor.DarkGreen);
                client.MessageReciveUse("( Conected to Public Chat )");

                string? line;
                do
                {
                    line = reader.ReadLine();

                    if (!string.IsNullOrEmpty(line))
                    {
                        client.Log(line, MessageType.PublicChat);
                        client.MessageReciveUse(line);
                    }

                } while (!string.IsNullOrEmpty(line));
            
            }
            finally 
            {
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.PublicChatFalse);
                client.SendMessage("Press ( Enter ) disconnecting...", MessageType.InformationMessege, MessegeClientInfo.MenuTrue);

                client.Log($"Disconected from Public Chat", MessageType.InformationMessege, ConsoleColor.DarkRed);
                client.MessageReciveUse("( Disconected from Public Chat )");

                reader.ReadLine();
                
            }
        }


        [MenuSubmenu("PrivateChat", 2, "Connect to private chat room or create your")]
        public PrivateChatSubMenu PrivateChat { get; set; }


        [MenuAction("ChangeName", 3, "Here you can change your NikName, other people will see it")]
        public void ChangeName(ChatClient client)
        {
            client.SendMessage(null, MessageType.ClearClientConsole);
            client.SendMessage("Press any button to continue", MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            
            var test = Task.CompletedTask;
            test = Task.Run(() =>
            {
                bool isChanged = false;
                string? newNickName;
                var reader = new StreamReader(client.TcpClient.GetStream());
                reader.ReadLine();
                client.SendMessage(null, MessageType.ClearClientConsole);

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

                } while (!isChanged || newNickName == string.Empty);
            
                if (isChanged) client.ChangeNameTo(newNickName);

                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuTrue);
                reader.ReadLine();
            });
            test.Wait();
            
        }
    }
}