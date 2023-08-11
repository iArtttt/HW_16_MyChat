using MyChatLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;


namespace MyChatServer
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await Server.Start();
        }
    }
}