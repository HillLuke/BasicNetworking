namespace shared
{
    public interface IMessagePacket : IPacket
    {
        string From { get; set; }
        string Message { get; set; }
    }
}
