using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using shared;

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
            server.BeginAcceptTcpClient(new AsyncCallback(AcceptTCP), server);

            Console.WriteLine("Server started");
            Console.Read();
        }

        private static void AcceptTCP(IAsyncResult asyncResult)
        {
            TcpListener listener = (TcpListener)asyncResult.AsyncState;
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTCP), listener);

            TcpClient client = listener.EndAcceptTcpClient(asyncResult);
            int id = _clients.Count;
            _clients.Add(id, client);

            Thread thread = new Thread(Handler);
            thread.Start(id);

            Console.WriteLine($"Connection from {client.Client.RemoteEndPoint}");
        }

        private static bool IsDisconnected(TcpClient tcpClient)
        {
            try
            {                
                return tcpClient.Client.Poll(10 * 1000, SelectMode.SelectRead) && (tcpClient.Client.Available == 0);
            }
            catch (SocketException se)
            {
                return true;
            }
        }

        private static void Handler(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = _clients[id];
            NetworkStream stream = client.GetStream();

            try
            {
                while (true)
                {
                    if (IsDisconnected(client))
                    {
                        lock (_lock) _clients.Remove(id);
                        client.Client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        BroadcastToAllButSender("User disconnected", id);
                        Console.WriteLine($"Disconnected {id}");
                        break;
                    }

                    if (client.Available > 0)
                    {

                        byte[] lengthBuffer = new byte[2];
                        stream.Read(lengthBuffer, 0, 2);
                        ushort packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);

                        byte[] jsonBuffer = new byte[packetByteSize];
                        stream.Read(jsonBuffer, 0, jsonBuffer.Length);

                        string jsonString = Encoding.UTF8.GetString(jsonBuffer);
                        IPacket packet = packet = Packet.Deserialize(jsonString);
                    }
                }
            }
            catch (Exception)
            {
                if (!client.Connected)
                {
                    lock (_lock) _clients.Remove(id);
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    BroadcastToAllButSender("User disconnected", id);
                    Console.WriteLine($"Err Disconnected {id}");
                }
            }
        }

        public static void BroadcastToAllButSender(string data, int id)
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
