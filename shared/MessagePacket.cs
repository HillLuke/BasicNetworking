namespace shared
{
    public class MessagePacket : Packet, IMessagePacket
    {
        public string From { get; set; }
        public string Message { get; set; }

        public MessagePacket() : base()
        {
            Command = ECommand.Message;
        }

    }
}
