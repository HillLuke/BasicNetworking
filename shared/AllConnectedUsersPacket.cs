using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public class AllConnectedUsersPacket : Packet, IAllConnectedUsersPacket
    {
        public List<string> Usernames { get; set; }

        public AllConnectedUsersPacket() : base()
        {
            Command = ECommand.AllConnectedUsers;
        }
    }
}
