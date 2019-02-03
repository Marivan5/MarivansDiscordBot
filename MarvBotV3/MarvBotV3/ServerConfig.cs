using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;

namespace MarvBotV3
{
    public class ServerConfig
    {
        public ulong videoChannel { get; set; }
        public ulong publicChannel { get; set; }
        public ulong serverOwner { get; set; }
        public ulong afkChannel { get; set; }


        public ServerConfig()
        {
            videoChannel = 0;
            publicChannel = 0;
            serverOwner = 117628335516942343;
            afkChannel = 0;
        }

        // Save the configuration to the specified file location.
        public void Save(string dir = "Data/serverConfiguration.json")
        {
            File.WriteAllText(dir, ToJson());
        }

        // Load the configuration from the specified file location.
        public static ServerConfig Load(string dir = "Data/serverConfiguration.json")
            => JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(dir));

        // Convert the configuration to a json string. 
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
