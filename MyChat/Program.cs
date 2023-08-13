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
        private static async Task Main(string[] args)
        {
            var client = new Client();
            await client.Start();
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