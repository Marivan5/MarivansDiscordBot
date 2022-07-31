using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MarvBotV3
{
    public class ServerConfig
    {
        public ulong videoChannel { get; set; }
        public List<ulong> publicChannel { get; set; }
        public ulong serverOwner { get; set; }
        public ulong afkChannel { get; set; }
        public ulong richRole { get; set; }
        public List<ulong> whiteList { get; set; }
        public List<string> videoList { get; set; }
        public int donationWaitHours { get; set; }
        public List<string> blacklistWords { get; set; }
        private int botUpdateTimer;
        public int BotUpdateTimer
        {
            get { return botUpdateTimer; }
            set
            {
                if (botUpdateTimer == value)
                    return;

                botUpdateTimer = value;
                OnPropertyChanged();
            }
        }
        private int goldToEveryoneTimer;
        public int GoldToEveryoneTimer 
        { 
            get { return goldToEveryoneTimer; } 
            set 
            {
                if (goldToEveryoneTimer == value)
                    return;

                goldToEveryoneTimer = value;
                OnPropertyChanged(); 
            } 
        }

        private static string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/serverConfiguration.json");

        public static event PropertyChangedEventHandler PropertyChanged;

        public ServerConfig()
        {
            videoChannel = 0;
            publicChannel = new List<ulong>();
            serverOwner = 117628335516942343;
            afkChannel = 0;
            richRole = 0;
            donationWaitHours = 6;
            JsonConvert.SerializeObject(whiteList);
            JsonConvert.SerializeObject(videoList);
            JsonConvert.SerializeObject(blacklistWords);
            goldToEveryoneTimer = 10;
            botUpdateTimer = 1;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
