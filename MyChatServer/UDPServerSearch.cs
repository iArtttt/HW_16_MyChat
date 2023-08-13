using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyChatServer
{
    class UDPServerSearch
    {
        private readonly int _port;
        private readonly UdpClient resUDP;
        public UDPClientSearch ClientSearch { get; }
        public UDPServerSearch(UDPClientSearch clientSearch, int port = 5002)
        {
            bool isComplete = false;
            do
            {
                try
                {
                    _port = port;
                    resUDP = new UdpClient(_port);
                    isComplete = true;
                }
                catch { port++; }
            } while (!isComplete);
            ClientSearch = clientSearch;
        }


        public async Task Search(CancellationToken cancellationToken = default)
        {
            var udp = new UdpClient();
            
            var msg = Encoding.UTF8.GetBytes($"I`m new Server, port: {Server.Port}");
            var endPoint = new IPEndPoint(IPAddress.Broadcast, 5002);
            await udp.SendAsync(msg, endPoint, cancellationToken);
            
            var resendTask = Task.Run(async () => 
            {
                do
                {
                    var get = await udp.ReceiveAsync();
                    var getMess = Encoding.UTF8.GetString(get.Buffer);

                    var portStr = getMess[(getMess.IndexOf(":") + 1)..].Trim();
                    var port = int.Parse(portStr);
                    if (!Server.ServersList.Any(s => s.Port == port))
                    {
                        Console.WriteLine($"Find new server, port: {port}");
                        Server.ServersList.Add(new IPEndPoint(get.RemoteEndPoint.Address, port));
                    }


                } while (!cancellationToken.IsCancellationRequested);
            });

            do
            {
                var recivedData = await resUDP.ReceiveAsync(cancellationToken);
                var messege = Encoding.UTF8.GetString(recivedData.Buffer);

                if (messege.StartsWith("I`m new Server, port:"))
                {
                    var ports = messege[(messege.IndexOf(":") + 1)..].Split(':');

                    foreach( var p in ports)
                    {
                        var port = int.Parse(p);
                        if (!Server.ServersList.Any(s => s.Port == port))
                        {
                            Console.WriteLine($"Find new server, port: {port}");

                            var messegeToSend = string.Join(':', "I`m new Server, port:" + string.Join(':', Server.ServersList.Select(p => p.Port).ToArray()));
                            var bytesToSend = Encoding.UTF8.GetBytes(messegeToSend);

                            await udp.SendAsync(bytesToSend, recivedData.RemoteEndPoint, cancellationToken);

                            Server.ServersList.Add(new IPEndPoint(recivedData.RemoteEndPoint.Address, port));

                            for (int i = 0; i < Server.AllClientsList.Count; i++)
                            {
                                var mess = Encoding.UTF8.GetBytes($"I`m Server, port: {port}");
                                await ClientSearch.Send(bytesToSend, Server.AllClientsList[i], cancellationToken);
                            }
                        }
                    }
                }
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
}
