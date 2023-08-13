using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyChatServer
{
    class UDPClientSearch
    {
        private readonly int _port;
        public readonly UdpClient udpFromClient;
        public UDPClientSearch(int port = 6002)
        {
            bool isComplete = false;
            do
            {
                try
                {
                    _port = port;
                    udpFromClient = new UdpClient(_port);
                    isComplete = true;
                }
                catch { port++; }
            }while (!isComplete);
        }
        public async Task Send(byte[] mes, IPEndPoint iPEndPoint, CancellationToken cancellationToken = default)
        {
            await udpFromClient.SendAsync(mes, iPEndPoint, cancellationToken);
        }
        public async Task Listen(CancellationToken cancellationToken = default)
        {
            
            do
            {

                var recivedData = await udpFromClient.ReceiveAsync(cancellationToken);
                var messege = Encoding.UTF8.GetString(recivedData.Buffer);

                if (messege == "Who is Server?")
                {
                    var ld = string.Join(':', "I`m Server, port:" + string.Join(':',Server.ServersList.Select(p => p.Port).ToArray()));
                    var mes = Encoding.UTF8.GetBytes(ld);
                    await udpFromClient.SendAsync(mes, recivedData.RemoteEndPoint, cancellationToken);

                    Server.AllClientsList.Add(recivedData.RemoteEndPoint);
                }
                
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
}
