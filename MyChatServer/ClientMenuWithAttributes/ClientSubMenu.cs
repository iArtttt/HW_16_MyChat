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

                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.ClearClientConsole);
                client.SendMessage("Press any button", MessageType.InformationMessege, MessegeClientInfo.MenuFalse);

                reader.ReadLine();
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.ClearClientConsole);
            
                client.SendMessage("Wait a few second connecting...", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
                Thread.Sleep(200);
            
                client.SendMessage("If you want to leave public chat press ( Enter )", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("You are in public chat now", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("Change Public Chat Access", MessageType.InformationMessege, MessegeClientInfo.Information);
            
                client.Log($"Conected to Public Chat", ConsoleColor.DarkGreen);
                client.MessageReciveUse("( Conected to Public Chat )");

                string? line = null;
                do
                {
                    line = reader.ReadLine();

                    if (!string.IsNullOrEmpty(line))
                    {
                        client.Log(line);
                        client.MessageReciveUse(line);
                    }

                } while (!string.IsNullOrEmpty(line));
            
            }
            finally 
            {
                client.SendMessage("Change Public Chat Access", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("Press ( Enter ) disconnecting...", MessageType.InformationMessege, MessegeClientInfo.MenuTrue);

                client.Log($"Disconected from Public Chat", ConsoleColor.DarkRed);
                client.MessageReciveUse("( Disconected from Public Chat )");

                reader.ReadLine();
                
            }
        }


        [MenuSubmenu("PrivateChat", 2, "Connect to private chat room or create your")]
        public PrivateChatSubMenu PrivateChat { get; set; }


        [MenuAction("ChangeName", 3, "Here you can change your NikName, other people will see it")]
        public void ChangeName(ChatClient client)
        {
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.MenuFalse);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.ClearClientConsole);
            client.SendMessage("Press any button to continue", MessageType.InformationMessege, MessegeClientInfo.Information);
            
            var test = Task.CompletedTask;
            test = Task.Run(() =>
            {
                bool isChanged = false;
                string? newNickName;
                var reader = new StreamReader(client.TcpClient.GetStream());
                reader.ReadLine();
                client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.ClearClientConsole);
            
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
                client.SendMessage("Press ( Enter ) to continue", MessageType.InformationMessege, MessegeClientInfo.Information);
                reader.ReadLine();
            });
            test.Wait();
            
        }
    }
}