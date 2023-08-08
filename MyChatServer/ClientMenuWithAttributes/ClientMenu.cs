using System.Reflection.PortableExecutable;

namespace MyChatServer
{
    internal class ClientMainMenu
    {
        [MenuSubmenu("LogIn", 1, "Everyone do it =)")]
        public ClientSubMenu SubMenu { get; set; }

        //[MenuSubmenu("Registration", 2, "Everyone must do it =)")]
        //public ClientRegMenu RegMenu { get; set; }

        [MenuAction("Disconect", int.MaxValue, "Please don`t leave us =(")]
        public void Disconect(ChatClient client) 
        {
            client.Dispose();
        }

    }

    //internal class ClientRegMenu
    //{
    //    [MenuSubmenu("Reg", 1)]
    //    public MyClass MyClasss { get; set; }
    //    public class MyClass
    //    {

    //        [MenuAction("PropOne", 1)]
    //        public void PropOne(ChatClient client)
    //        {
    //            Console.WriteLine("PropOne");
    //            PropTwo(client);
    //        }
    //        [MenuAction("PropTwo", 2)]
    //        public void PropTwo(ChatClient client)
    //        {
    //            Console.WriteLine("PropTwo");
    //            PropThree();
    //        }
    //        private void PropThree()
    //        {
    //            Console.WriteLine("PropThree");
    //        }
    //    }
    //}
}