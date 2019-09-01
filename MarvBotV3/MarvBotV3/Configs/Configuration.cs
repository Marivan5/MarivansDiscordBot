using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace MarvBotV3
{
    public class Configuration
    {
        // Your bot's command prefix. Please don't pick `!`. 
        public char Prefix { get; set; }
        // Ids of users who will have owner access to the bot.
        public ulong Owner { get; set; }
        // Your bot's login token.
        public string Token { get; set; }

        private static string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/configuration.json");
        public Configuration()
        {
            Prefix = '!';
            Owner = 117628335516942343;
            Token = "";
        }

        // Save the configuration to the specified file location.
        public void Save()
        {
            File.WriteAllText(dir, ToJson());
        }

        // Load the configuration from the specified file location.
        public static Configuration Load()
            => JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(dir));

        // Convert the configuration to a json string. 
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
