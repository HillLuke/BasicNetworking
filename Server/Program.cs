using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
    class Program
    {

        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> _clients = new Dictionary<int, TcpClient>();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");

            IPAddress localAddress = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddress, 8001);

            server.Start();
            server.BeginAcceptTcpClient(new AsyncCallback(acceptTCP), server);

            Console.WriteLine("Server started");
            Console.Read();
        }

        private static void acceptTCP(IAsyncResult asyncResult)
        {
            TcpListener listener = (TcpListener)asyncResult.AsyncState;
            listener.BeginAcceptTcpClient(new AsyncCallback(acceptTCP), listener);

            TcpClient client = listener.EndAcceptTcpClient(asyncResult);
            int id = _clients.Count;
            _clients.Add(id, client);

            Thread thread = new Thread(handler);
            thread.Start(id);

            Console.WriteLine($"Connection from {client.Client.RemoteEndPoint}...");
        }

        private static void handler(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = _clients[id];

            try
            {
                while (true)
                {
                    if (!client.Connected)
                    {
                        break;
                    }

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int byte_count = stream.Read(buffer, 0, buffer.Length);

                    if (byte_count == 0)
                    {
                        break;
                    }

                    string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                    broadcastToAllButSender(data, id);
                    Console.WriteLine(data);
                }
            }
            catch (Exception)
            {
                //TODO
            }

            lock (_lock) _clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void broadcastToAllButSender(string data, int id)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (KeyValuePair<int, TcpClient> c in _clients)
                {
                    if (c.Key == id)
                    {
                        //Dont echo to the sender
                        continue;
                    }

                    NetworkStream stream = c.Value.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
