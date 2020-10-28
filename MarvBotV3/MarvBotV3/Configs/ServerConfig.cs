using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MarvBotV3
{
    public class ServerConfig
    {
        public ulong videoChannel { get; set; }
        public ulong publicChannel { get; set; }
        public ulong serverOwner { get; set; }
        public ulong afkChannel { get; set; }
        public ulong richRole { get; set; }
        public List<ulong> whiteList { get; set; }
        public List<string> videoList { get; set; }
        //public int maxGambles { get; set; }

        private static string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/serverConfiguration.json");

        public ServerConfig()
        {
            videoChannel = 0;
            publicChannel = 0;
            serverOwner = 117628335516942343;
            afkChannel = 0;
            richRole = 0;
            //maxGambles = 10;
            JsonConvert.SerializeObject(whiteList);
            JsonConvert.SerializeObject(videoList);
        }

        // Save the configuration to the specified file location.
        public void Save()
        {
            File.WriteAllText(dir, ToJson());
            try
            {
                Program.serverConfig = Load();
            }
            catch
            {
                Console.WriteLine("Cant load config.");
            }
        }

        // Load the configuration from the specified file location.
        public static ServerConfig Load()
            => JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(dir));

        // Convert the configuration to a json string. 
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
