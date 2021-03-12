 using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Program
    {
        static string ID;
        static string Username;

        static void Main(string[] args)
        {
            ID = Guid.NewGuid().ToString();

            Console.WriteLine("Enter Username");
            Username = Console.ReadLine();
            Console.WriteLine($"Username set as {Username}");

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 8001;
            TcpClient client = new TcpClient();
            client.Connect(ip, port);
            Console.WriteLine($"Connected as {ID}");
            NetworkStream ns = client.GetStream();
            Thread thread = new Thread(o => ReceiveData((TcpClient)o));
            thread.Start(client);


            Thread spamText = new Thread(o => spam((NetworkStream)o));
            //spamText.Start(ns);

            string s;
            while (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                byte[] buffer = Encoding.ASCII.GetBytes($"{Username} : {s}");
                ns.Write(buffer, 0, buffer.Length);
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            spamText.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("disconnect from server!!");
            Console.ReadKey();
        }

        static void spam(NetworkStream stream)
        {
            while (true)
            {
                byte[] buffer = Encoding.ASCII.GetBytes($"Hello from {ID}");
                stream.Write(buffer, 0, buffer.Length);
                Thread.Sleep(500);
            }
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }
    }
}
