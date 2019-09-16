using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using MarvBotV3.Database;

namespace MarvBotV3
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        public readonly DiscordShardedClient _discord;
        private readonly IServiceProvider _services;

        public static List<SocketUser> freeMsgList = new List<SocketUser>();
        private char prefix = Configuration.Load().Prefix;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            //_commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
            _discord.GuildMemberUpdated += ChangeGameAndRole;
            _discord.UserVoiceStateUpdated += ChangeVoiceChannel;
            GiveGoldToEveryone();
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if (Program.serverConfig.whiteList == null || Program.serverConfig.whiteList.All(x => x != message.Author.Id))
            {
                if (Program.serverConfig.publicChannel != message.Channel.Id) // Special channel that does not follow the normal rules
                {
                    if (Program.serverConfig.videoChannel != 0)
                    {
                        if (Program.serverConfig.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id == Program.serverConfig.videoChannel)
                        {
                            if (!freeMsgList.Contains(message.Author))
                            {
                                freeMsgList.Add(message.Author);
                            }
                            Emoji thumbsUp = new Emoji("👍");
                            await message.AddReactionAsync(thumbsUp);
                        }
                        else if (Program.serverConfig.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id != Program.serverConfig.videoChannel)
                        {
                            if (!freeMsgList.Contains(message.Author))
                            {
                                freeMsgList.Add(message.Author);
                            }
                            ulong videoChan = Program.serverConfig.videoChannel;
                            await message.DeleteAsync();
                            await message.Channel.SendMessageAsync("Please don't post videos in this channel. I have posted it for you in " + MentionUtils.MentionChannel(videoChan));
                            var cchannel = (message.Channel as SocketGuildChannel)?.Guild;
                            var textChannel = (ISocketMessageChannel)cchannel.GetChannel(videoChan);
                            await textChannel.SendMessageAsync(message + " Posted by: " + message.Author.Mention);
                        }
                        else if (!Program.serverConfig.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id == Program.serverConfig.videoChannel)
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
            if (!message.HasCharPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                return;
            var context = new ShardedCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result.Error.HasValue)
                await context.Channel.SendMessageAsync(result.ToString());
        }

        public async Task ChangeGameAndRole(SocketGuildUser beforeChangeUser, SocketGuildUser afterChangeUser)
        {
            SocketGuildUser user = afterChangeUser;
            IRole gameRole = null;
            SocketGuild guild = user.Guild;
            if(afterChangeUser.Activity != null)
            {
                if(beforeChangeUser.Activity != null)
                {
                    if(beforeChangeUser.Activity.Name == afterChangeUser.Activity.Name)
                    {
                        return;
                    }
                    gameRole = guild.Roles.Where(input => input.ToString().Equals(beforeChangeUser.Activity.Name)).FirstOrDefault();
                    await user.RemoveRoleAsync(gameRole);
                    gameRole = null;
                }
                gameRole = guild.Roles.Where(input => input.ToString().Equals(afterChangeUser.Activity.Name)).FirstOrDefault();
                if (gameRole == null) // if role does not exist, create it
                {
                    gameRole = await guild.CreateRoleAsync(afterChangeUser.Activity.Name, permissions: GuildPermissions.None, color: Color.Default, isHoisted: false);
                }
                Discord.Rest.RestVoiceChannel altChannel = null;
                var channel = guild.VoiceChannels.Where(input => input.ToString().Equals(gameRole.Name)).FirstOrDefault();
                if (channel == null)
                {
                    VoiceChannelProperties properties = new VoiceChannelProperties();
                    properties.Bitrate = 96000;
                    altChannel = await guild.CreateVoiceChannelAsync(gameRole.Name);
                    await altChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
                    await altChannel.AddPermissionOverwriteAsync(gameRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow));
                    await altChannel.ModifyAsync(x => x.Bitrate = properties.Bitrate); 
                    //await user.ModifyAsync(x => x.Channel = altChannel); // Flyttar användaren
                }
                else
                {
                    //await user.ModifyAsync(x => x.Channel = channel); // Flyttar användaren
                }
                await user.AddRoleAsync(gameRole);
            }
            else if (beforeChangeUser.Activity != null && afterChangeUser.Activity == null)
            {
                gameRole = guild.Roles.Where(input => input.ToString().Equals(beforeChangeUser.Activity.Name)).FirstOrDefault();
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

        //public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        //{
        //    // command is unspecified when there was a search failure (command not found); we don't care about these errors
        //    if (!command.IsSpecified)
        //        return;

        //    // the command was succesful, we don't care about this result, unless we want to log that a command succeeded.
        //    if (result.IsSuccess)
        //        return;

        //    // the command failed, let's notify the user that something happened.
        //    await context.Channel.SendMessageAsync($"error: {result.ToString()}");
        //}

        List<string> msges = new List<string>() { "Please undefean yourself before joining a voice channel.", "To join a voice channel you have to undeafen yourself." };
        List<string> rareMsges = new List<string>() { "Bög", "I love you! <3", "Please don't hate me",
            "I'm not a bot, I am a chinese worker living in Shenzen and I have been forced to work by the Chinese goverment. They have a deal with Marivan to force us to work for nothing. Please send help!" };

        public async Task ChangeVoiceChannel(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            SocketGuild guild = afterState.VoiceChannel.Guild;
            SocketGuildUser guildUser = guild.GetUser(user.Id);
            SocketVoiceChannel afkChannel = guild.GetVoiceChannel(Program.serverConfig.afkChannel);

            if (guildUser.VoiceState.Value.IsSelfDeafened && Program.serverConfig.whiteList.All(x => x != user.Id) && !afterState.VoiceChannel.Equals(afkChannel)) // Moves self muted users to afk channel
            {
                if (afkChannel != null)
                {
                    await guildUser.ModifyAsync(x => x.Channel = afkChannel);
                    var rnd = new Random().Next(0, 101);
                    if(rnd > 99)
                    {
                        var rng = new Random().Next(0, rareMsges.Count());
                        await guildUser.SendMessageAsync(rareMsges[rng]);
                    }
                    else
                    {
                        var rng = new Random().Next(0, msges.Count());
                        await guildUser.SendMessageAsync(msges[rng]);
                    }
                }
            }
        }

        public async Task GiveGoldToEveryone()
        {
            var millisecs = Convert.ToInt32(TimeSpan.FromMinutes(10).TotalMilliseconds);
            while(true)
            {
                var guilds = _discord.Guilds;
                List<SocketGuildUser> users = new List<SocketGuildUser>();
                foreach (var guild in guilds)
                {
                    users = users.Concat(guild.Users).ToList();
                }
                var usersOnline = users.Where(x => x.Status != UserStatus.Offline && !x.IsBot).ToList();
                await DataAccess.GiveGoldEveryone(usersOnline, 10);
                await Task.Delay(Convert.ToInt32(millisecs));
            }
        }
    }
}
