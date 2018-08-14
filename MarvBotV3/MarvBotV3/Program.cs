using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;

namespace MarvBotV3
{
    class Program
    {
        private DiscordSocketClient client;
        private CommandService commands;

        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private async Task Start()
        {
            EnsureConfigExists();

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });

            commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
            });

            client.MessageReceived += Client_MessageReceived;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.Ready += Client_Ready;
            client.Log += Client_Log;

            await client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage arg)
        {
            Console.WriteLine($"[{DateTime.Now} at {arg.Source}] {arg.Message}");
            
        }

        private async Task Client_Ready()
        {
            throw new NotImplementedException();
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            Console.WriteLine($"User: {message.Author} Sent message: {message}");

            string msg = message.ToString();
        }

        public static void EnsureConfigExists()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Data")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));

            string loc = Path.Combine(AppContext.BaseDirectory, "Data/configuration.json");

            if (!File.Exists(loc))                              // Check if the configuration file exists.
            {
                var config = new Configuration();               // Create a new configuration object.

                Console.WriteLine("The configuration file has been created at 'Data\\configuration.json', " + "please enter your information and restart MarvBot.");
                Console.Write("Token: ");

                config.Token = Console.ReadLine();              // Read the bot token from console.
                config.Save();                                  // Save the new configuration object to file.
            }
            Console.WriteLine("Configuration Loaded...");
        }
    }
}
