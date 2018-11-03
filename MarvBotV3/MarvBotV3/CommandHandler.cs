using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace MarvBotV3
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public static IUserMessage lastNotCommand = null;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.MessageReceived += MessageReceivedAsync;
            _discord.GuildMemberUpdated += ChangeGameAndRole;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if(message.Author.Id != ServerConfig.Load().serverOwner)
            {
                if (ServerConfig.Load().publicChannel != message.Channel.Id) // Special channel that does not follow the normal rules
                {
                    if (ServerConfig.Load().videoChannel != 0)
                    {
                        if (Program.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id != ServerConfig.Load().videoChannel)
                        {
                            ulong videoChan = ServerConfig.Load().videoChannel;
                            await message.DeleteAsync();
                            await message.Channel.SendMessageAsync("Please don't post videos in this channel. I have posted it for you in " + MentionUtils.MentionChannel(videoChan));
                            var cchannel = (message.Channel as SocketGuildChannel)?.Guild;
                            var textChannel = (ISocketMessageChannel)cchannel.GetChannel(videoChan);
                            await textChannel.SendMessageAsync(message + " Posted by: " + message.Author.Mention);
                        }
                        else if (!Program.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id == ServerConfig.Load().videoChannel)
                        {
                            await message.DeleteAsync();
                            //await message.Channel.SendMessageAsync("Please only post videos in this channel");
                            await message.Author.SendMessageAsync("Please only post videos in the video channel");
                        }
                    }
                }
            }

            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasCharPrefix(Configuration.Load().Prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                if(message.Channel.GetMessageAsync(message.Id) != null)
                {
                    lastNotCommand = message;
                    return;
                } 
            }

            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result.Error.HasValue)
                await context.Channel.SendMessageAsync(result.ToString());
        }

        public async Task ChangeGameAndRole(SocketGuildUser beforeChangeUser, SocketGuildUser afterChangeUser)
        {
            SocketGuildUser user = afterChangeUser;
            IRole gameRole = null;
            SocketGuild guild = user.Guild;
            if(afterChangeUser.Game != null)
            {
                if(beforeChangeUser.Game != null)
                {
                    if(beforeChangeUser.Game.Value.Name == afterChangeUser.Game.Value.Name)
                    {
                        return;
                    }
                    gameRole = guild.Roles.Where(input => input.ToString().Equals(beforeChangeUser.Game.Value.Name)).FirstOrDefault();
                    await user.RemoveRoleAsync(gameRole);
                    gameRole = null;
                }
                gameRole = guild.Roles.Where(input => input.ToString().Equals(afterChangeUser.Game.Value.Name)).FirstOrDefault();
                if (gameRole == null) // if role does not exist, create it
                {
                    gameRole = await guild.CreateRoleAsync(afterChangeUser.Game.Value.Name, permissions: GuildPermissions.None, color: Color.Default, isHoisted: false);
                }
                Discord.Rest.RestVoiceChannel altChannel = null;
                var channel = guild.VoiceChannels.Where(input => input.ToString().Equals(gameRole.Name)).FirstOrDefault();
                if (channel == null)
                {
                    VoiceChannelProperties properties = new VoiceChannelProperties();
                    properties.Bitrate = 96000;
                    altChannel = await guild.CreateVoiceChannelAsync(gameRole.Name);
                    await altChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny));
                    await altChannel.AddPermissionOverwriteAsync(gameRole, new OverwritePermissions(connect: PermValue.Allow));
                    await altChannel.ModifyAsync(x => x.Bitrate = properties.Bitrate); 
                    //await user.ModifyAsync(x => x.Channel = altChannel);
                }
                else
                {
                    //await user.ModifyAsync(x => x.Channel = channel);
                }
                await user.AddRoleAsync(gameRole);
            }
            else if (beforeChangeUser.Game != null && afterChangeUser.Game == null)
            {
                gameRole = guild.Roles.Where(input => input.ToString().Equals(beforeChangeUser.Game.Value.Name)).FirstOrDefault();
                Discord.Rest.RestVoiceChannel altChannel = null;
                SocketVoiceChannel channel = guild.VoiceChannels.Where(input => input.ToString().Equals("General")).FirstOrDefault();
                if (channel == null)
                {
                    altChannel = await guild.CreateVoiceChannelAsync("General");
                    //await user.ModifyAsync(x => x.Channel = altChannel);
                }
                else
                {
                    //await user.ModifyAsync(x => x.Channel = channel);
                }
                await user.RemoveRoleAsync(gameRole);
            }
        }
    }
}
