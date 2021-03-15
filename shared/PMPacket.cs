namespace shared
{
    public class PMPacket : Packet, IPMPacket
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }

        public PMPacket() : base()
        {
            Command = ECommand.PM;
        }

    }
}
