using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public interface IAllConnectedUsersPacket
    {
        public List<string> Usernames { get; set; }
    }
}
