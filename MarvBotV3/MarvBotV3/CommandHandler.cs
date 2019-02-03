using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace MarvBotV3
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public static IUserMessage lastNotCommand = null;
        public static List<SocketUser> freeMsgList = new List<SocketUser>();


        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _discord.MessageReceived += MessageReceivedAsync;
            _discord.GuildMemberUpdated += ChangeGameAndRole;
            _discord.UserVoiceStateUpdated += ChangeVoiceChannel;
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
                        if(Program.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id == ServerConfig.Load().videoChannel)
                        {
                            if(!freeMsgList.Contains(message.Author))
                            {
                                freeMsgList.Add(message.Author);
                            }
                            Emoji thumbsUp = new Emoji("👍");
                            await message.AddReactionAsync(thumbsUp);
                        }
                        else if (Program.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id != ServerConfig.Load().videoChannel)
                        {
                            if (!freeMsgList.Contains(message.Author))
                            {
                                freeMsgList.Add(message.Author);
                            }
                            ulong videoChan = ServerConfig.Load().videoChannel;
                            await message.DeleteAsync();
                            await message.Channel.SendMessageAsync("Please don't post videos in this channel. I have posted it for you in " + MentionUtils.MentionChannel(videoChan));
                            var cchannel = (message.Channel as SocketGuildChannel)?.Guild;
                            var textChannel = (ISocketMessageChannel)cchannel.GetChannel(videoChan);
                            await textChannel.SendMessageAsync(message + " Posted by: " + message.Author.Mention);
                        }
                        else if (!Program.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id == ServerConfig.Load().videoChannel)
                        {
                            if (freeMsgList.Contains(message.Author))
                            {
                                freeMsgList.Remove(message.Author);
                            }
                            else
                            {
                                await message.DeleteAsync();
                                //await message.Channel.SendMessageAsync("Please only post videos in this channel");
                                await message.Author.SendMessageAsync("Please only post videos in the video channel");
                            }
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
                    await altChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, readMessages: PermValue.Deny));
                    await altChannel.AddPermissionOverwriteAsync(gameRole, new OverwritePermissions(connect: PermValue.Allow, readMessages: PermValue.Allow));
                    await altChannel.ModifyAsync(x => x.Bitrate = properties.Bitrate); 
                    //await user.ModifyAsync(x => x.Channel = altChannel); // Flyttar användaren
                }
                else
                {
                    //await user.ModifyAsync(x => x.Channel = channel); // Flyttar användaren
                }
                await user.AddRoleAsync(gameRole);
            }
            else if (beforeChangeUser.Game != null && afterChangeUser.Game == null)
            {
                gameRole = guild.Roles.Where(input => input.ToString().Equals(beforeChangeUser.Game.Value.Name)).FirstOrDefault();
                //Discord.Rest.RestVoiceChannel altChannel = null;
                //SocketVoiceChannel channel = guild.VoiceChannels.Where(input => input.ToString().Equals("General")).FirstOrDefault();
                //if (channel == null)
                //{
                //    altChannel = await guild.CreateVoiceChannelAsync("General");
                //    await user.ModifyAsync(x => x.Channel = altChannel);
                //}
                //else
                //{
                //    await user.ModifyAsync(x => x.Channel = channel);
                //}
                await user.RemoveRoleAsync(gameRole);
            }
        }

        public async Task ChangeVoiceChannel(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            SocketGuild guild = afterState.VoiceChannel.Guild;
            SocketGuildUser guildUser = guild.GetUser(user.Id);
            SocketVoiceChannel channel = guild.GetVoiceChannel(ServerConfig.Load().afkChannel);

            if (guildUser.VoiceState.Value.IsSelfDeafened && user.Id != ServerConfig.Load().serverOwner && !afterState.VoiceChannel.Equals(channel)) // Moves self muted users to afk channel
            {
                if (channel != null)
                {
                    await guildUser.ModifyAsync(x => x.Channel = channel);
                    await guildUser.SendMessageAsync("Please undefean yourself before joining a voice channel.");
                }
            }
        }
    }
}
