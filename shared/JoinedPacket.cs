using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class JoinedPacket : Packet, IJoinedPacket
    {
        public string Username { get; set; }

        public JoinedPacket() : base()
        {
            Command = ECommand.Joined;
        }
    }
}
