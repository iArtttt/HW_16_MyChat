namespace MyChatServer
{
    internal class ClientMainMenu
    {
        [MenuSubmenu("LogIn", 1)]
        public ClientSubMenu SubMenu { get; set; }

        [MenuSubmenu("Registration", 2)]
        public ClientRegMenu RegMenu { get; set; }
        //public void Registration()
        //{
        //    Console.WriteLine("Reg");

        //}
        [MenuAction("Disconect", int.MaxValue)]
        public void Disconect() 
        {
            
        }

    }
    internal class ClientRegMenu
    {
        [MenuAction("PropOne", 1)]
        public void PropOne()
        {

        }
        [MenuAction("PropTwo", 2)]
        public void PropTwo()
        {

        }
    }
    internal class ClientSubMenu
    {
        [MenuAction("PublicChat", 1)]
        public void PublicChat()
        {
            //var task = Task.CompletedTask;
            //task = Task.Run(() =>
            //{
            //    string? line = null;
            //    do
            //    {
            //        line = reader.ReadLine();


            //        client.Log(line);
            //        client.MessageReciveUse(line);




            //    } while (!string.IsNullOrEmpty(line));
            //});
        }

        [MenuAction("PrivateChat", 2)]
        public void PrivateChat()
        {

        }
        [MenuAction("ChangeName", 3)]
        public void ChangeName()
        {

        }
    }
}