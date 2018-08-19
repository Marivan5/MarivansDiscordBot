using Newtonsoft.Json;
using System.IO;

namespace MarvBotV3
{
    public class Configuration
    {
        // Your bot's command prefix. Please don't pick `!`. 
        public char Prefix { get; set; }
        // Ids of users who will have owner access to the bot.
        public ulong[] Owners { get; set; }
        // Your bot's login token.
        public string Token { get; set; }

        public Configuration()
        {
            Prefix = ' ';
            Owners = new ulong[] { 0 };
            Token = "";
        }

        // Save the configuration to the specified file location.
        public void Save(string dir = "Data/configuration.json")
        {
            File.WriteAllText(dir, ToJson());
        }

        // Load the configuration from the specified file location.
        public static Configuration Load(string dir = "Data/configuration.json")
            => JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(dir));

        // Convert the configuration to a json string. 
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
