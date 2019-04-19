using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace MarvBotV3
{
    class Program
    {
        private DiscordSocketClient client;
        //public static List<string> videoList = new List<string>() { "youtube.com", "vimeo.com", "liveleak.com", "youtu.be", "clips.twitch.tv" };
        public static ServerConfig serverConfig; 

        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private async Task Start()
        {
            EnsureBotConfigExists();
            EnsureServerConfigExists();
            serverConfig = ServerConfig.Load();
            var services = ConfigureServices();

            //client = new DiscordSocketClient(new DiscordSocketConfig
            //{
            //    LogLevel = LogSeverity.Debug
            //});

            client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;

            services.GetRequiredService<CommandService>().Log += LogAsync;

            //commands = new CommandService(new CommandServiceConfig
            //{
            //    CaseSensitiveCommands = true,
            //    DefaultRunMode = RunMode.Async,
            //    LogLevel = LogSeverity.Debug
            //});

            //client.MessageReceived += Client_MessageReceived;
            //await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            //client.Ready += Client_Ready;

            await client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await client.StartAsync();
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }


        //private async Task Client_Ready()
        //{
        //    throw new NotImplementedException();
        //}

        //private async Task Client_MessageReceived(SocketMessage _message) // Not active due to services
        //{
        //    Console.WriteLine($"User: {_message.Author} Sent message: {_message}");

        //    SocketUserMessage message = _message as SocketUserMessage;
        //    SocketCommandContext context = new SocketCommandContext(client, message);

        //    if (context.Message.Content.Length > 0 || context.User.IsBot)
        //        return;
        //}

        public static void EnsureBotConfigExists()
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

        public static void EnsureServerConfigExists()
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Data")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));

            string loc = Path.Combine(AppContext.BaseDirectory, "Data/serverConfiguration.json");

            if (!File.Exists(loc))                              // Check if the configuration file exists.
            {
                var config = new ServerConfig();                // Create a new configuration object.

                config.Save();                                  // Save Config
            }
            Console.WriteLine("Server configuration Loaded...");
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                //.AddSingleton<Program>()
                .BuildServiceProvider();
        }
    }
}
