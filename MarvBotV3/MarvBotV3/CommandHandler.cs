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
using MarvBotV3.DTO;
using System.Globalization;

namespace MarvBotV3
{
    public class CommandHandler
    {
        private NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " " };
        public static CommandService _commands;
        public readonly DiscordShardedClient _discord;
        private readonly IServiceProvider _services;

        public static List<SocketUser> freeMsgList = new List<SocketUser>();
        private char prefix = Configuration.Load().Prefix;

        DataAccess da;
        MarvBotBusinessLayer bl;

        public CommandHandler(IServiceProvider services)
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);

            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            //_commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
            _discord.GuildMemberUpdated += ChangeGameAndRole;
            _discord.UserJoined += UserJoined;
            _discord.UserLeft += UserLeft;
            _discord.UserVoiceStateUpdated += ChangeVoiceChannel;
            //_ = RunIntervalTask();

            SetTimer();
        }

        private Task UserLeft(SocketGuildUser arg)
        {
            DataAccess da = new DataAccess(new DatabaseContext());
            return da.DeleteUser(arg.Id);
        }

        private Task UserJoined(SocketGuildUser arg)
        {
            var role = arg.Guild.GetRole(349580645502025728); // Intruder
            return arg.AddRoleAsync(role);
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
                if (!Program.serverConfig.publicChannel.Contains(message.Channel.Id)) // Special channel that does not follow the normal rules
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

            var context = new ShardedCommandContext(_discord, message);

            foreach (var duel in Program.awaitingDuels)
            {
                if (duel.TimeStamp < DateTime.Now.AddMinutes(-1))
                    Program.awaitingDuels.Remove(duel);
            }

            if (Program.awaitingDuels.Select(x => x.Challenge).Contains(rawMessage.Author.Id) && rawMessage.Content.ToLower().Split(" ").Contains("yes"))
            {
                var duel = Program.awaitingDuels.Last(x => x.Challenge == rawMessage.Author.Id);
                var challenger = duel.Challenger;
                await context.Channel
                    .SendMessageAsync($"{rawMessage.Author.Mention} has accepted {MentionUtils.MentionUser(challenger)}s call to duel. Be ready to shoot (🔫).");
                Program.awaitingDuels.Remove(duel);
                _ = CountDownInChat(context.Channel, challenger, rawMessage.Author.Id, duel.BetAmount);
            }

            if ((Program.activeDuels.Select(x => x.Challenge).Contains(rawMessage.Author.Id) 
                || Program.activeDuels.Select(x => x.Challenger).Contains(rawMessage.Author.Id)) 
                && rawMessage.Content.ToLower().Split(" ").Contains("🔫"))
            {
                var duel = Program.activeDuels.Last(x => x.Challenge == rawMessage.Author.Id || x.Challenger == rawMessage.Author.Id);
                var loser = duel.Challenge == rawMessage.Author.Id ? duel.Challenger : duel.Challenge;
                await context.Channel.SendMessageAsync($"{rawMessage.Author.Mention} has won {duel.BetAmount.ToString("n0", nfi)} of {MentionUtils.MentionUser(loser)} gold");
                Program.activeDuels.Remove(duel);
                await bl.SaveGold(rawMessage.Author, context.Guild, duel.BetAmount);
                await bl.SaveGold(context.Guild.GetUser(loser), context.Guild, -duel.BetAmount);
                await da.SetDuel(duel.Challenger, duel.Challenge, rawMessage.Author.Id, duel.BetAmount);
            }

            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasCharPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                return;
            
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result.Error.HasValue)
                await context.Channel.SendMessageAsync(result.ToString());
        }

        private async Task CountDownInChat(ISocketMessageChannel textChat, ulong challenger, ulong challenge, int betAmount)
        {
            var rnd = new Random();
            for(int i = 3; i > 0; i--)
            {
                await textChat.SendMessageAsync(i.ToString());
                await Task.Delay(rnd.Next(1001, 1500));
            }
            await textChat.SendMessageAsync("Shoot! (🔫)");
            Program.activeDuels.Add(new Duel { Challenger = challenger, Challenge = challenge, BetAmount = betAmount, TimeStamp = DateTime.Now });
        }

        public async Task ChangeGameAndRole(SocketGuildUser beforeChangeUser, SocketGuildUser afterChangeUser)
        {
            SocketGuildUser user = afterChangeUser;
            IRole gameRole = null;
            SocketGuild guild = user.Guild;

            if (!string.IsNullOrWhiteSpace(beforeChangeUser.Activity?.Name) || !string.IsNullOrWhiteSpace(afterChangeUser.Activity?.Name))
                if (beforeChangeUser.Activity?.Name != afterChangeUser.Activity?.Name)
                    await bl.SaveUserAcitivity(user, beforeChangeUser.Activity?.Name ?? "", afterChangeUser.Activity?.Name ?? "");

            if(afterChangeUser.Activity != null)
            {
                if(beforeChangeUser.Activity != null)
                {
                    if(beforeChangeUser.Activity.Name == afterChangeUser.Activity.Name)
                        return;

                    gameRole = guild.Roles.Where(x => x.ToString().Equals(beforeChangeUser.Activity.Name) && !x.IsMentionable).FirstOrDefault();
                    await DeleteGameRoleAndVoiceChannel(guild, gameRole, user);
                    gameRole = null;
                }
                gameRole = guild.Roles.Where(x => x.ToString().Equals(afterChangeUser.Activity.Name) && !x.IsMentionable).FirstOrDefault();

                if (gameRole == null) // if role does not exist, create it
                    gameRole = await guild.CreateRoleAsync(afterChangeUser.Activity.Name, permissions: GuildPermissions.None, color: Color.Default, isHoisted: false, false);

                Discord.Rest.RestVoiceChannel altChannel = null;
                var channel = guild.VoiceChannels.Where(x => x.ToString().Equals(gameRole.Name) && x.Bitrate == 96000).FirstOrDefault();
                if (channel == null)
                {
                    var properties = new VoiceChannelProperties
                    {
                        Bitrate = 96000
                    };
                    altChannel = await guild.CreateVoiceChannelAsync(gameRole.Name);
                    await altChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
                    await altChannel.AddPermissionOverwriteAsync(gameRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow));
                    await altChannel.ModifyAsync(x => x.Bitrate = properties.Bitrate); 
                    //await user.ModifyAsync(x => x.Channel = altChannel); // Flyttar användaren
                }
                //else
                //{
                //    await user.ModifyAsync(x => x.Channel = channel); // Flyttar användaren
                //}
                await user.AddRoleAsync(gameRole);
            }
            else if (beforeChangeUser.Activity != null && afterChangeUser.Activity == null)
            {
                gameRole = guild.Roles.Where(x => x.ToString().Equals(beforeChangeUser.Activity.Name) && !x.IsMentionable).FirstOrDefault();

                if (gameRole == null)
                    return;

                await DeleteGameRoleAndVoiceChannel(guild, gameRole, user);
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
            }
        }

        private async Task DeleteGameRoleAndVoiceChannel(SocketGuild guild, IRole gameRole, SocketGuildUser user)
        {
            await user.RemoveRoleAsync(gameRole);

            if (!guild.Users.Where(x => x != user).Any(x => x.Roles.Contains(gameRole))) // raderar
            {
                SocketVoiceChannel channel = guild.VoiceChannels.Where(x => x.ToString().Equals(gameRole.Name) && x.Bitrate == 96000).FirstOrDefault();
                await gameRole.DeleteAsync();
                await channel.DeleteAsync();
            }
        }

        List<string> msges = new List<string>() { "Please undefean yourself before joining a voice channel.", "To join a voice channel you have to undeafen yourself." };
        List<string> rareMsges = new List<string>() { "Bög", "I love you! <3", "Please don't hate me",
            "I'm not a bot, I am a chinese worker living in Shenzen and I have been forced to work by the Chinese goverment. They have a deal with Marivan to force us to work for nothing. Please send help!" };

        public async Task ChangeVoiceChannel(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
        {
            SocketGuild guild = afterState.VoiceChannel?.Guild;
            if(guild == null)
                guild = beforeState.VoiceChannel.Guild;

            SocketGuildUser guildUser = guild.GetUser(user.Id);
            SocketVoiceChannel afkChannel = guild.GetVoiceChannel(Program.serverConfig.afkChannel);

            if (guildUser.VoiceState == null)
                return;

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

        private System.Timers.Timer aTimer;
        readonly int millisecs = Convert.ToInt32(TimeSpan.FromMinutes(10).TotalMilliseconds);

        private void SetTimer()
        {
            aTimer = new System.Timers.Timer(millisecs);
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await CelebrateBirthday();
            await GiveGoldToEveryone();
            Console.WriteLine($"Loop at: {DateTime.Now}");
        }

        //public async Task RunIntervalTask()
        //{
        //    CancellationToken cancellationToken = new CancellationToken(); // Todo move to commands

        //    await Task.Run(async () =>
        //    {
        //        var i = 0;
        //        try
        //        {
        //            while (true)
        //            {
        //                i++;
        //                await CelebrateBirthday();
        //                await GiveGoldToEveryone();
        //                await Task.Delay(millisecs, cancellationToken);
        //                Console.WriteLine($"{DateTime.Now} Givegoldeveryone: {i}");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message);
        //        }
        //    }, cancellationToken);
        //}

        public async Task CelebrateBirthday()
        {
            var birthdays = await da.GetTodaysBirthdaysWithoutGift();
            if (!birthdays.Any())
                return;

            var guilds = _discord.Guilds;
            
            foreach (var guild in guilds)
            {
                foreach (var birthday in birthdays)
                {
                    if (guild.Users.Select(x => x.Id).Contains(birthday.UserID))
                    {
                        await guild.DefaultChannel
                            .SendMessageAsync($":tada: Happy birthday {MentionUtils.MentionUser(birthday.UserID)} :tada:");
                        await da.UpdateBirthdayLastGiftGiven(guild.GetUser(birthday.UserID), DateTime.Now);
                    }
                }
            }
        }

        public async Task GiveGoldToEveryone()
        {
            var guilds = _discord.Guilds;
            var users = new List<SocketGuildUser>();
            var extraGoldUsers = new List<SocketGuildUser>();
            foreach (var guild in guilds)
            {
                var onlineUsers = guild.Users.Where(x => !x.IsSelfDeafened && x.Status == UserStatus.Online && !x.IsBot).ToList();
                onlineUsers.Remove(bl.GetCurrentRichestPerson(guild));
                users.AddRange(onlineUsers);
                var userActivities = onlineUsers.GroupBy(x => new { x.Activity?.Name })
                    .Where(x => x.Key.Name != null && x.Count() > 1 && x.Key.Name != "Custom Status")
                    .Select(x => x.ToList());

                foreach (var act in userActivities)
                    extraGoldUsers.AddRange(act);
            }
            await da.GiveGoldEveryone(users, 1);
            await da.GiveGoldEveryone(extraGoldUsers, 2);
        }
    }
}
