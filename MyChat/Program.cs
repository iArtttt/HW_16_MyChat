using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using MyChatLib;
using MyChatServer;


namespace MyChat
{
    internal class Program
    {

        private static bool isMenu = true;
        private static bool isInPublicChat = false;
        private static string Name = string.Empty;

        private static async Task Main(string[] args)
        {
            using var tcpClient = new TcpClient(AddressFamily.InterNetwork);
            tcpClient.Connect(IPAddress.Loopback, 5002);

            Console.WriteLine($"Client started on [{tcpClient.Client.LocalEndPoint}]");
            Console.WriteLine($"Connected to [{tcpClient.Client.RemoteEndPoint}]");


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
                        try
                        {
                            var splited = line.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                            if (int.TryParse(splited[0], out int res))
                            {
                                switch (res)
                                {
                                    case (byte)MessageType.InformationMessege:
                                    
                                        Information(splited);
                                    
                                        break;
                                
                                    case (byte)MessageType.PublicChat:

                                        PublicChat(splited, tcpClient);

                                        break;
                                
                                    case (byte)MessageType.PrivateChat:

                                        PrivateChat(splited, tcpClient);
                                        
                                        break;

                                    case (byte)MessageType.ClearClientConsole:

                                        Console.Clear();

                                        break;

                                }
                            }
                        }
                        catch { }

                    }

                } while (!reader.EndOfStream);
            });


            var writerTask = Task.Run(() =>
            {
                do
                {
                    try
                    {
                        if(isMenu)
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

        }
        
        private static void Information(string[] splited)
        {
            if (int.TryParse(splited[1], out int infoType))
            {
                switch (infoType)
                {
                    case (byte)MessegeClientInfo.Allert:

                        Print(splited, 2, ConsoleColor.Red);

                        break;
                    
                    case (byte)MessegeClientInfo.Succed:

                        Print(splited, 2, ConsoleColor.Green);

                        break;
                    
                    case (byte)MessegeClientInfo.Information:

                        Print(splited, 2, ConsoleColor.Yellow);

                        break;
                    
                    
                    case (byte)MessegeClientInfo.MenuTrue:
                        
                        isMenu = true;
                        
                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Please press ( Enter ) to continue...");
                        }

                        break;
                    
                    case (byte)MessegeClientInfo.MenuFalse:
                        
                        isMenu = false;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("Please press ( Any Button ) to continue...");
                        }

                        break;
                    case (byte)MessegeClientInfo.PublicChatTrue:

                        isInPublicChat = true;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);

                        break;
                    
                    case (byte)MessegeClientInfo.PublicChatFalse:

                        isInPublicChat = false;

                        if (splited.Skip(2).FirstOrDefault() != null)
                            Print(splited, 2, ConsoleColor.Yellow);

                        break;
                    
                    case (byte)MessegeClientInfo.ChangeName:
                        
                        Name = (splited.Skip(3).FirstOrDefault() != null)
                            ? splited.Skip(2).Aggregate((f, c) => f + " " + c) 
                            : splited[2];

                        break;
                    default:

                        Print(splited, 2, ConsoleColor.White);
                        
                        break;
                }
            }
            else
            {
                Print(splited);
            }
        }
        public static void PublicChat(string?[] splited, TcpClient tcpClient)
        {

            if (Name != string.Empty)
            {
                if (splited[1].ToString() != $"{Name}" && isInPublicChat)
                    Print(splited);
            }
            else if (splited[1] != tcpClient.Client.LocalEndPoint.ToString() && isInPublicChat)
                    Print(splited);
        }
        public static void PrivateChat(string?[] splited, TcpClient tcpClient)
        {

            if (int.TryParse(splited[1], out int infoType))
            {
                switch (infoType)
                {
                    case (byte)MessegeClientInfo.Allert:

                        Print(splited, 2, ConsoleColor.DarkRed);

                        break;


                    case (byte)MessegeClientInfo.Succed:

                        Print(splited, 2, ConsoleColor.DarkGreen);

                        break;

                    case (byte)MessegeClientInfo.Information:

                        Print(splited, 2, ConsoleColor.DarkYellow);
                        
                        break;

                    default:

                        Print(splited, ConsoleColor.Blue);

                        break;
                }
            }
            else 
            {
                Print(splited, ConsoleColor.DarkBlue);
            }
        }

        private static void Print(string?[] splited, ConsoleColor consoleColor) => Print(splited, 1, consoleColor);
        private static void Print(string?[] splited, int skipelements = 1) => Print(splited, skipelements, ConsoleColor.Gray);
        private static void Print(string?[] splited, int skipelements, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            
            if (splited.Skip(skipelements + 1).FirstOrDefault() != null)
                Console.WriteLine(splited.Skip(skipelements).Aggregate((f, c) => f + " " + c));
            else if (splited.Skip(skipelements).FirstOrDefault() != null)
                Console.WriteLine(splited[skipelements]);
            else 
                Console.WriteLine();
            
            Console.ResetColor();
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