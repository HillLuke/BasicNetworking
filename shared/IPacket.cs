namespace shared
{
    public interface IPacket
    {
        ECommand Command { get; }

        public string Serialize();
    }
}
