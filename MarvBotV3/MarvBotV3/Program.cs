﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MarvBotV3.Dto;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace MarvBotV3;

class Program
{
    public static ServerConfig serverConfig;
    public static List<Duel> awaitingDuels = new List<Duel>();
    public static List<Duel> activeDuels = new List<Duel>();
    public static List<RockPaperScissors> activeRockPaperScissorsEvents = new List<RockPaperScissors>();
    public static NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " ", CurrencyDecimalSeparator = "." };

    public static void Main(string[] args) =>
        new Program().Start().GetAwaiter().GetResult();

    private async Task Start()
    {
        var config = new DiscordSocketConfig
        {
            TotalShards = 1,
            GatewayIntents = GatewayIntents.All,
            AlwaysDownloadUsers = true
        };

        EnsureBotConfigExists();
        EnsureServerConfigExists();
        serverConfig = ServerConfig.Load();

        using (var services = ConfigureServices(config))
        {
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            await services.GetRequiredService<CommandHandler>().InitializeAsync();
            await client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

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

    private ServiceProvider ConfigureServices(DiscordSocketConfig config)
    {
        return new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(config))
            .AddSingleton<InteractionService>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }
}
