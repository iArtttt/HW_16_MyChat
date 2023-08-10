using MyChatLib;

namespace MyChatServer
{
    internal class ClientHelpMenu
    {
        [MenuAction("How to move", 1, "Press ( Enter ) to open")]
        public void HowToMove(ChatClient client)
        {
            var reader = new StreamReader(client.TcpClient.GetStream());

            client.SendMessage(null, MessageType.ClearClientConsole);
            client.SendMessage("====( How to move Tutorial )====", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("To move ( UP )      ==> Press 'W' or 'UpArrow'", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("To move ( Down )    ==> Press 'S' or 'DownArrow'", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("To move ( Forward ) ==> Press 'D' or 'RightArrow' or 'Enter'", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("To move ( Back )    ==> Press 'A' or 'LeftArrow' or 'Backspace'", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            

            PropThree(client, 1);

            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("Press any button to leave", MessageType.InformationMessege, MessegeClientInfo.Information);
            reader.ReadLine();
        }
        [MenuAction("Spesial", 2)]
        public void Spesial(ChatClient client)
        {
            var reader = new StreamReader(client.TcpClient.GetStream());

            client.SendMessage(null, MessageType.ClearClientConsole);
            client.SendMessage("====( Spesial )====", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("There is nothing heare yet", MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            
            PropThree(client, 2);

            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage(null, MessageType.InformationMessege, MessegeClientInfo.Information);
            client.SendMessage("Press any button to leave", MessageType.InformationMessege, MessegeClientInfo.Information);
            reader.ReadLine();
        }
        private bool isAllTutorsComplete = false;
        private bool isTutorOneComplete = false;
        private bool isTutorTwoComplete = false;

        private void PropThree(ChatClient client, int tutorWached)
        {
            if (isAllTutorsComplete)
            {
                client.SendMessage("====( ------------------------------------ )====", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("====( You have already watch all Tutorials )====", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("====( ------------------------------------ )====", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("====(        Don`t come hear again!        )====", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("====( ------------------------------------ )====", MessageType.InformationMessege, MessegeClientInfo.Information);
            }
            else if (tutorWached == 1)
            {
                isTutorOneComplete = true;
                client.Log("Watch ( How to move ) tutorial", MessageType.InformationMessege, ConsoleColor.DarkGray);
            }
            else if (tutorWached == 2)
            {
                isTutorTwoComplete = true;
                client.Log("Watch ( Spesial ) tutorial", MessageType.InformationMessege, ConsoleColor.DarkGray);
            }
            if (isTutorOneComplete && isTutorTwoComplete && !isAllTutorsComplete)
            {
                isAllTutorsComplete = true;
                
                client.Log("( Complete All Tutorial Watching )", MessageType.InformationMessege, ConsoleColor.DarkGray);

                client.SendMessage("====( ------------------------------------ )====", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("====( You have already watch all Tutorials )====", MessageType.InformationMessege, MessegeClientInfo.Information);
                client.SendMessage("====( ------------------------------------ )====", MessageType.InformationMessege, MessegeClientInfo.Information);
            }
        }
    }
}