using System.Net;
using System.Net.Sockets;
using System.Text;
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
                        if (splited[0] == "Log+@!_NeW0nAmE:")
                        {
                            name = splited.Skip(1).Aggregate((f, s) => f + s).ToString();
                            writer.Flush();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"You change your NickName to {name}");
                            Console.ResetColor();
                        }
                        else if (splited[0] == "#Menu#" && splited[1] == tcpClient.Client.LocalEndPoint.ToString())
                        {
                            try
                            {
                                Console.WriteLine(splited.Skip(2).Aggregate((f,c) => f + " " + c));
                            }
                            catch { Console.WriteLine(); }
                        }
                        //foreach (var item in splited)
                        //{
                        //    Console.WriteLine(item);
                        //}
                        //Console.WriteLine(splited[0]);
                        //Console.WriteLine(tcpClient.Client.LocalEndPoint.ToString());
                        //Console.WriteLine(splited[0] != tcpClient.Client.LocalEndPoint.ToString());
                        else
                        {
                            if (name != string.Empty)
                            {
                                if (splited[0].ToString().CompareTo(name) < 0)
                                    Console.WriteLine(line);
                            }
                            else if (splited[0] != tcpClient.Client.LocalEndPoint.ToString())
                                Console.WriteLine(line);
                        }
                    }

                } while (!reader.EndOfStream);
            });


            var writerTask = Task.Run(() =>
            {
                do
                {
                    var line = Console.ReadLine();
                    //stream.WriteByte((byte)MessageType.System);
                    writer.WriteLine(line);
                    writer.Flush();
                }
                while (stream.CanWrite);
            });

            Task.WaitAll(readerTask, writerTask);
        }
    }
}