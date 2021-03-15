using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class DisconnectedPacket : Packet, IJoinedPacket
    {
        public string Username { get; set; }

        public DisconnectedPacket() : base()
        {
            Command = ECommand.Disconnected;
        }
    }
}
