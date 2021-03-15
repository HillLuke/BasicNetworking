namespace shared
{
    public interface IPacket
    {
        ECommand Command { get; set; }

        public string Serialize();
    }
}
