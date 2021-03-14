using Newtonsoft.Json;

namespace shared
{
    public class Packet : IPacket
    {
        public ECommand Command { get; set; }

        public string Serialize()
        {
            var indented = Formatting.Indented;
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            return JsonConvert.SerializeObject(this, indented, settings);
        }
        public static IPacket Deserialize(string json)
        {
            // https://skrift.io/issues/bulletproof-interface-deserialization-in-jsonnet/
            
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            return (IPacket)JsonConvert.DeserializeObject(json, settings);
        }
    }

    public enum ECommand
    {
        Echo,
        Message
    }
}
