using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public string Username { get; set; }

        public int ID { get; set; }

        public TcpClient TcpClient { get; set; }
    }
}
