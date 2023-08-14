using System.Reflection.PortableExecutable;

namespace MyChatServer
{
    internal class ClientMainMenu
    {

        [MenuSubmenu("Help", 1, "Press ( Enter ) to open")]
        public ClientHelpMenu RegMenu { get; set; }

        [MenuSubmenu("LogIn", 2, "Everyone do it =)")]
        public ClientSubMenu SubMenu { get; set; }

        [MenuAction("Disconect", int.MaxValue, "Please don`t leave us =(")]
        public void Disconect(ChatClient client) 
        {
            client.Dispose();
        }

    }
}