using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using MyChatLib;


namespace MyChat
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var tcpClient = new TcpClient(AddressFamily.InterNetwork);
            tcpClient.Connect(IPAddress.Loopback, 5002);

            Console.WriteLine($"Client started on {tcpClient.Client.LocalEndPoint}");
            Console.WriteLine($"Connected to " + tcpClient.Client.RemoteEndPoint);

            bool isMenu = true;
            var name = string.Empty;
            var stream = tcpClient.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);

            var readerTask = Task.Run(() =>
            {
                string? line = null;
                do
                {
                    line = reader.ReadLine();
                    if (stream.CanRead)
                    {

                        var splited = line.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        if (int.TryParse(splited[0], out int res))
                        {
                            switch (res)
                            {
                                case (byte)MessageType.InformationMessege:
                                    
                                    Information(splited, ref isMenu);
                                    
                                    break;
                                
                                case (byte)MessageType.Menu:
                                    
                                    Menu(splited, tcpClient);

                                    break;
                                
                                case (byte)MessageType.PublicChat:

                                    PublicChat(splited, name, line, tcpClient);

                                    break;
                                
                                case (byte)MessageType.PrivateChat:
                                    
                                    break;
                                
                            }
                        }

                    }

                } while (!reader.EndOfStream);
            });


            var writerTask = Task.Run(() =>
            {
                do
                {
                    try
                    {
                        if (isMenu)
                        {
                            var line = Convert.ToInt32(Console.ReadKey().Key);
                            writer.WriteLine(line);
                            writer.Flush();
                        }
                        else
                        {
                            var line = Console.ReadLine();
                            writer.WriteLine(line);
                            writer.Flush();
                        }
                    }
                    catch { stream.Close(); }
                }
                while (stream.CanWrite);
            });

            Task.WaitAll(readerTask, writerTask);
            Console.WriteLine("\n\nGoodLuck!\n\n");
        }

        private static void Information(string[] splited, ref bool isMenu)
        {
            if (int.TryParse(splited[1], out int infoType))
            {
                switch (infoType)
                {
                    case (byte)MessegeClientInfo.Clear:

                        Console.Clear();
                        
                        break;
                    case (byte)MessegeClientInfo.Succed:

                        Console.ForegroundColor = ConsoleColor.Green;

                        Print(splited, 2);

                        Console.ResetColor();
                        
                        break;
                    case (byte)MessegeClientInfo.Information:

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Print(splited, 2);
                        Console.ResetColor();

                        break;
                    case (byte)MessegeClientInfo.Allert:
                        
                        Console.ForegroundColor = ConsoleColor.Red;
                        Print(splited, 2);
                        Console.ResetColor();

                        break;
                    case (byte)MessegeClientInfo.MenuTrue:
                        isMenu = true;
                        break;
                    case (byte)MessegeClientInfo.MenuFalse:
                        isMenu = false;
                        break;
                }


            }
        }
        private static void Print(string?[] splited, int skipelements = 1)
        {
            if (splited.Skip(skipelements + 1).FirstOrDefault() != null)
                Console.WriteLine(splited.Skip(1).Aggregate((f, c) => f + " " + c));
            else
                Console.WriteLine(splited[skipelements]);
        }

        private static void Menu(string[] splited, TcpClient tcpClient)
        {

            if (splited.Skip(2).FirstOrDefault() != null)
                Console.WriteLine(splited.Skip(1).Aggregate((f, c) => f + " " + c));
            else if (splited.Skip(1).FirstOrDefault() != null)
                Console.WriteLine(splited[1]);
            else
                Console.WriteLine();

        }

        public static void PublicChat(string?[] splited, string name, string line, TcpClient tcpClient)
        {
            if (name != string.Empty)
            {
                if (splited[1].ToString().CompareTo(name) < 0)
                    Print(splited);
            }
            else if (splited[1] != tcpClient.Client.LocalEndPoint.ToString())
                    Print(splited);
        }
    }
}
//foreach (var item in splited)
//{
//    Console.WriteLine(item);
//}
//Console.WriteLine(splited[0]);
//Console.WriteLine(tcpClient.Client.LocalEndPoint.ToString());
//Console.WriteLine(splited[0] != tcpClient.Client.LocalEndPoint.ToString());