using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyChat
{
    public class ServersSearch
    {
        public int Port { get; }
        public ServersSearch(int port = 6002)
        {
            Port = port;
        }

        public async Task SearchAsync(List<IPEndPoint> servers, CancellationToken cancellationToken = default)
        {
            UdpClient udpClient = new UdpClient();

            var msg = Encoding.UTF8.GetBytes("Who is Server?");
            var endPoint = new IPEndPoint(IPAddress.Broadcast, Port);

            await udpClient.SendAsync(msg, endPoint, cancellationToken);
        
            var recive = await udpClient.ReceiveAsync(cancellationToken);
            
            var reciveMessage = Encoding.UTF8.GetString(recive.Buffer);
            int port = -1;
            if (reciveMessage.StartsWith("I`m Server, port:"))
            {
                var ports = reciveMessage[(reciveMessage.IndexOf(":") + 1)..].Split(':');
                foreach (var p in ports)
                {
                    port = int.Parse(p.Trim());
                    if (!servers.Any(s => s.Port == port))
                    {
                        servers.Add(new IPEndPoint(recive.RemoteEndPoint.Address, port));
                        Console.WriteLine("I Find new server");
                    }
                }
            }

        }

    }
}
