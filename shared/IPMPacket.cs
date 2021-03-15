using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    public interface IPMPacket : IPacket
    {
        string From { get; set; }
        string To { get; set; }
        string Message { get; set; }
    }
}
