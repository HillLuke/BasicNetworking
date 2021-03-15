namespace shared
{
    public class PMPacket : Packet, IMessagePacket
    {
        public string From { get; set; }
        string To { get; set; }
        public string Message { get; set; }

        public PMPacket() : base()
        {
            Command = ECommand.PM;
        }

    }
}
