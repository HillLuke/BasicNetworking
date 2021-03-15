 using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using shared;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Client
{
    class Program
    {
        static TcpClient _client;
        static IPAddress _ip;

        static string _ID;
        static string _username;
        static int _port;
        static bool _isConnected;
        static Queue<string> _messageQueue = new Queue<string>();
        static Dictionary<ECommand, Func<IPacket, Task>> _commandHandlers = new Dictionary<ECommand, Func<IPacket, Task>>();

        static Thread _recieveData;
        static Thread _sendData;
        static Thread _handleInput;

        static readonly object _messageQueueLock = new object();

        static void Main(string[] args)
        {
            _ID = Guid.NewGuid().ToString();

            Console.WriteLine("Enter Username");
            _username = Console.ReadLine();
            Console.WriteLine($"Username set as {_username}");

            _ip = IPAddress.Parse("127.0.0.1");
            _port = 8001;
            _client = new TcpClient();

            try
            {
                _client.Connect(_ip, _port);
                _isConnected = true;

                _commandHandlers[ECommand.Message] = RecieveMessage;
                _commandHandlers[ECommand.Joined] = RecieveUserJoined;
                _commandHandlers[ECommand.Disconnected] = RecieveUserDisconnected;
                _commandHandlers[ECommand.AllConnectedUsers] = RecieveAllConnectedUsers;
                _commandHandlers[ECommand.PM] = RecievePMPacket;

                SendUserJoined();

                Console.WriteLine($"Connected as {_ID}");
            }
            catch (Exception)
            {
                Console.WriteLine($"Error connecting");
                Console.ReadLine();
                return;
            }

            _recieveData = new Thread(o => ReceiveData());
            _recieveData.Start(_client);
            _sendData = new Thread(o => SendData());
            _sendData.Start(_client);
            _handleInput = new Thread(o => HandleInput());
            _handleInput.Start(_client);
        }

        static string GetInput()
        {
            return Console.ReadLine();
        }

        static void Close()
        {
            _isConnected = false;
            _recieveData.Interrupt();
            _sendData.Interrupt();
            _handleInput.Interrupt();
            _client.Close();
            Console.WriteLine("Disconnected from server, press any key to exit.");
            Console.ReadKey();
        }

        static async void ReceiveData()
        {
            NetworkStream stream = _client.GetStream();

            while (_isConnected)
            {
                try
                {
                    if (_client.Available > 0)
                    {
                        byte[] lengthBuffer = new byte[2];
                        stream.Read(lengthBuffer, 0, 2);
                        ushort packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);

                        byte[] jsonBuffer = new byte[packetByteSize];
                        stream.Read(jsonBuffer, 0, jsonBuffer.Length);

                        string jsonString = Encoding.UTF8.GetString(jsonBuffer);
                        IPacket packet = Packet.Deserialize(jsonString);

                        await _commandHandlers[packet.Command](packet);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        static void SendData()
        {

            while (_isConnected)
            {
                if (_messageQueue.Count > 0)
                {
                    IPacket packet = new MessagePacket
                    {
                        Message = _messageQueue.Peek(),
                        From = _username
                    };

                    Send(packet);

                    lock (_messageQueueLock)
                    {
                        _messageQueue.Dequeue();
                    }
                }
            }
        }

        static void HandleInput()
        {
            while (_isConnected)
            {
                string input = GetInput();

                if (string.IsNullOrEmpty(input))
                {
                    SendUserDisconnected();
                    Close();
                }

                if (input.Equals("/commands"))
                {
                    Console.WriteLine("/online - gets usernames of all connected users");
                    Console.WriteLine("/pm <username> <message> - sends message to username only");
                }
                else if (input.Equals("/online"))
                {
                    SendAllConnectedUsers();
                }
                else if (input.StartsWith("/pm"))
                {
                    var regex = new Regex(@"(pm*)\s(\S*)\s(.*)");
                    var match = regex.Match(input);
                    SendPMPacket(match.Groups[2].Value, match.Groups[3].Value);
                }
                else
                {
                    lock (_messageQueueLock)
                    {
                        _messageQueue.Enqueue(input);
                    }
                }
            }
        }

        static void Send(IPacket packet)
        {
            NetworkStream stream = _client.GetStream();

            byte[] jsonBuffer = Encoding.UTF8.GetBytes(packet.Serialize());
            byte[] lengthBuffer = BitConverter.GetBytes(Convert.ToUInt16(jsonBuffer.Length));

            byte[] packetBuffer = new byte[lengthBuffer.Length + jsonBuffer.Length];
            lengthBuffer.CopyTo(packetBuffer, 0);
            jsonBuffer.CopyTo(packetBuffer, lengthBuffer.Length);

            stream.Write(packetBuffer, 0, packetBuffer.Length);
        }

        #region Send

        public static void SendUserJoined()
        {
            Send(new JoinedPacket
            {
                Username = _username
            });
        }

        public static void SendUserDisconnected()
        {
            Send(new DisconnectedPacket
            {
                Username = _username
            });
        }

        public static void SendAllConnectedUsers()
        {
            Send(new AllConnectedUsersPacket());
        }

        public static void SendPMPacket(string to, string message)
        {
            Send(new PMPacket
            {
                From = _username,
                Message = message,
                To = to
            });
        }

        #endregion

        #region Recieve

        public static Task RecieveMessage(IPacket packet)
        {
            Console.WriteLine($"{((MessagePacket)packet).From} : {((MessagePacket)packet).Message}");
            return Task.CompletedTask;
        }

        public static Task RecieveUserDisconnected(IPacket packet)
        {
            Console.WriteLine($"{((DisconnectedPacket)packet).Username} has left");
            return Task.CompletedTask;
        }

        public static Task RecieveUserJoined(IPacket packet)
        {
            Console.WriteLine($"{((JoinedPacket)packet).Username} has joined");
            return Task.CompletedTask;
        }

        public static Task RecieveAllConnectedUsers(IPacket packet)
        {
            Console.WriteLine("Connected users:");
            foreach (var username in ((AllConnectedUsersPacket)packet).Usernames)
            {
                Console.WriteLine($" {username}");
            }

            return Task.CompletedTask;
        }

        public static Task RecievePMPacket(IPacket packet)
        {
            Console.ForegroundColor
            = ConsoleColor.Yellow;
            Console.WriteLine($"{((PMPacket)packet).From} : {((PMPacket)packet).Message}", Console.ForegroundColor);
            Console.ForegroundColor
            = ConsoleColor.White;
            return Task.CompletedTask;
        }

        #endregion
    }
}
