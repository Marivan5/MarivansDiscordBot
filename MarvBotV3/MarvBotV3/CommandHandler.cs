﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using MarvBotV3.Database;
using MarvBotV3.Dto;

namespace MarvBotV3
{
    public class CommandHandler
    {
        public static CommandService _commands;
        public readonly DiscordShardedClient _discord;
        private readonly IServiceProvider _services;
        private int goldToEveryoneTimer = 10;
        DataAccess da;
        MarvBotBusinessLayer bl;

        public static List<SocketUser> freeMsgList = new List<SocketUser>();
        private char prefix = Configuration.Load().Prefix;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordShardedClient>();
            _services = services;
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);

            //_commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
            //_discord.GuildMemberUpdated += ChangeGameAndRole;
            _discord.PresenceUpdated += PresenceUpdated;
            _discord.UserJoined += UserJoined;
            _discord.UserLeft += UserLeft;
            _discord.UserVoiceStateUpdated += ChangeVoiceChannel;
            _discord.ButtonExecuted += ButtonExecuted;
            //_ = RunIntervalTask();

            ServerConfig.PropertyChanged += ServerConfig_PropertyChanged;
            goldToEveryoneTimer = Program.serverConfig.GoldToEveryoneTimer;

            SetTimer();

        }

        private void ServerConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ServerConfig.GoldToEveryoneTimer))
                goldToEveryoneTimer = Program.serverConfig.GoldToEveryoneTimer;
            else if (e.PropertyName == nameof(ServerConfig.BotUpdateTimer))
                aTimer.Interval = Convert.ToInt32(TimeSpan.FromMinutes(Program.serverConfig.BotUpdateTimer).TotalMilliseconds);
        }

        private async Task ButtonExecuted(SocketMessageComponent component)
        {
            await ExecuteDuelLogic(component);
            await ExecuteRockPaperScissors(component);
        }

        private async Task ExecuteRockPaperScissors(SocketMessageComponent component)
        {
            if (!component.Data.CustomId.StartsWith("rps_"))
                return;

            long rpsId = Convert.ToInt64(new string(component.Data.CustomId.Where(char.IsDigit).ToArray()));
            RockPaperScissors rps = Program.activeRockPaperScissorsEvents.FirstOrDefault(x => x.Id == rpsId);

            if (rps == null)
                return;

            if (rps.Challenge != component.User.Id && rps.Challenger != component.User.Id)
                return;

            string rpsChoice = new string(component.Data.CustomId.ToCharArray().Where(x => char.IsLetter(x) || char.IsPunctuation(x)).ToArray());
            var removeText = "rps_";
            var indexRemove = rpsChoice.IndexOf(removeText);
            rpsChoice = rpsChoice.Substring(indexRemove + removeText.Length);

            if (rpsChoice == "decline")
            {
                var buttons = new ComponentBuilder().WithButton("🥌", $"rps_rock", disabled: true).WithButton("🧻", $"rps_paper", disabled: true).WithButton("✂", $"rps_scissors", disabled: true).WithButton("Decline", $"rps_decline", disabled: true);
                if (rps.Challenger == component.User.Id)
                    await component.RespondAsync($"{MentionUtils.MentionUser(rps.Challenger)} has pussied out of their own challenge.");

                if (rps.Challenge == component.User.Id)
                    await component.RespondAsync($"{component.User.Mention} has declined {MentionUtils.MentionUser(rps.Challenger)}s call to Rock Paper Scissors.");

                Program.activeRockPaperScissorsEvents.Remove(rps);
                await component.Message.ModifyAsync(x => x.Components = buttons.Build());
                return;
            }

            if(rps.Challenge == component.User.Id)
                rps.ChallengeChoice = rpsChoice;
            else if (rps.Challenger == component.User.Id)
                rps.ChallengerChoice = rpsChoice;

            Program.activeRockPaperScissorsEvents.Remove(rps);
            Program.activeRockPaperScissorsEvents.Add(rps);

            if (string.IsNullOrWhiteSpace(rps.ChallengeChoice) || string.IsNullOrWhiteSpace(rps.ChallengerChoice))
                await component.RespondAsync($"{component.User.Mention} has made a choice.");

            if (!string.IsNullOrWhiteSpace(rps.ChallengeChoice) && !string.IsNullOrWhiteSpace(rps.ChallengerChoice))
            {
                ulong loser = 0;
                ulong winner = 0;
                bool isThereWinner = false;
                var winnerChoice = RockPaperScissorsTranslator(rpsChoice);
                var loserChoice = "";

                if ((rps.ChallengeChoice == "rock" && rps.ChallengerChoice == "scissors")
                    || (rps.ChallengeChoice == "scissors" && rps.ChallengerChoice == "paper")
                    || (rps.ChallengeChoice == "paper" && rps.ChallengerChoice == "rock"))
                {
                    isThereWinner = true;
                    loser = rps.Challenger;
                    winner = rps.Challenge;
                    winnerChoice = RockPaperScissorsTranslator(rps.ChallengeChoice);
                    loserChoice = RockPaperScissorsTranslator(rps.ChallengerChoice);
                }
                else if ((rps.ChallengerChoice == "rock" && rps.ChallengeChoice == "scissors")
                    || (rps.ChallengerChoice == "scissors" && rps.ChallengeChoice == "paper")
                    || (rps.ChallengerChoice == "paper" && rps.ChallengeChoice == "rock"))
                {
                    isThereWinner = true;
                    loser = rps.Challenge;
                    winner = rps.Challenger;
                    winnerChoice = RockPaperScissorsTranslator(rps.ChallengerChoice);
                    loserChoice = RockPaperScissorsTranslator(rps.ChallengeChoice);
                }

                var buttons = new ComponentBuilder().WithButton("🥌", $"rps_rock", disabled: true).WithButton("🧻", $"rps_paper", disabled: true).WithButton("✂", $"rps_scissors", disabled: true).WithButton("Decline", $"rps_decline", disabled: true);
                await component.Message.ModifyAsync(x => x.Components = buttons.Build());

                if (!isThereWinner)
                {
                    await component.RespondAsync($"There is no winner. Both took {winnerChoice}");
                    Program.activeRockPaperScissorsEvents.Remove(rps);
                    return;
                }

                await component.RespondAsync($"{MentionUtils.MentionUser(winner)} choose {winnerChoice}, while {MentionUtils.MentionUser(loser)} choose {loserChoice}. {MentionUtils.MentionUser(winner)} has won {rps.BetAmount.ToString("n0", Program.nfi)} of {MentionUtils.MentionUser(loser)} gold");

                if (rps.BetAmount > 0)
                {
                    SocketGuild guild = component.User.MutualGuilds.FirstOrDefault(x => x.Channels.Select(x => x.Id).ToList().Contains((ulong)component.ChannelId)); // Hack :(
                    await bl.SaveGold(guild.GetUser(winner), guild, rps.BetAmount);
                    await bl.SaveGold(guild.GetUser(loser), guild, -rps.BetAmount);
                    await da.SetRockPaperScissors(rps.Challenger, rps.Challenge, winner, rps.BetAmount, rps.ChallengerChoice, rps.ChallengeChoice);
                }

                Program.activeRockPaperScissorsEvents.Remove(rps);
                return;
            }
        }

        private string RockPaperScissorsTranslator(string input)
        {
            string output = "🥌";

            if (input == "paper")
                output = "🧻";
            else if (input == "scissors")
                output = "✂";

            return output;
        }

        private async Task ExecuteDuelLogic(SocketMessageComponent component)
        {
            foreach (var duel in Program.awaitingDuels)
            {
                if (duel.TimeStamp < DateTime.Now.AddMinutes(-1))
                {
                    Program.awaitingDuels.Remove(duel);
                    var duelId = Convert.ToInt64(new string(component.Data.CustomId.Where(Char.IsDigit).ToArray()));
                    var buttons = new ComponentBuilder().WithButton("Accept", "duel_accept", disabled: true).WithButton("Decline", "duel_decline", disabled: true);

                    if (component.Data.CustomId.StartsWith("duel_accept"))
                        duelId = Convert.ToInt64(component.Data.CustomId.Substring("duel_accept".Length));

                    if (duelId == duel.DuelId)
                        await component.Message.ModifyAsync(x => x.Components = buttons.Build());
                }
            }

            if (component.Data.CustomId.StartsWith("duel_"))
            {
                long duelId = Convert.ToInt64(new string(component.Data.CustomId.Where(Char.IsDigit).ToArray()));
                Duel duel = Program.activeDuels.FirstOrDefault(x => x.DuelId == duelId);

                if (component.Data.CustomId.StartsWith("duel_countdown"))
                    return;

                if (component.Data.CustomId.StartsWith("duel_shoot"))
                {
                    if (duel.Challenge == component.User.Id || duel.Challenger == component.User.Id)
                    {
                        var finalButton = new ComponentBuilder().WithButton("🔫", $"duel_shoot{duelId}", disabled: true);
                        await component.Message.ModifyAsync(x => x.Components = finalButton.Build());
                        var loser = duel.Challenge == component.User.Id ? duel.Challenger : duel.Challenge;
                        await component.RespondAsync($"{component.User.Mention} has won {duel.BetAmount.ToString("n0", Program.nfi)} of {MentionUtils.MentionUser(loser)} gold");
                        Program.activeDuels.Remove(duel);
                        SocketGuild guild = component.User.MutualGuilds.FirstOrDefault(x => x.Channels.Select(x => x.Id).ToList().Contains((ulong)component.ChannelId)); // Hack :(

                        if (duel.BetAmount > 0)
                        {
                            await bl.SaveGold(component.User, guild, duel.BetAmount);
                            await bl.SaveGold(guild.GetUser(loser), guild, -duel.BetAmount);
                            await da.SetDuel(duel.Challenger, duel.Challenge, component.User.Id, duel.BetAmount);
                        }
                    }
                    return;
                }

                var answer = "declined";
                var buttons = new ComponentBuilder().WithButton("Accept", "duel_accept", disabled: true).WithButton("Decline", "duel_decline", disabled: true);

                if (component.Data.CustomId.StartsWith("duel_accept"))
                {
                    duelId = Convert.ToInt64(component.Data.CustomId.Substring("duel_accept".Length));
                    answer = "accepted";
                }

                var challenger = duel.Challenger;

                if (component.Data.CustomId.StartsWith("duel_decline"))
                {
                    if (challenger == component.User.Id)
                    {
                        await component.RespondAsync($"{MentionUtils.MentionUser(challenger)} has pussied out of their own duel.");
                        Program.awaitingDuels.Remove(duel);
                        await component.Message.ModifyAsync(x => x.Components = buttons.Build());
                        return;
                    }
                }

                if (duel.Challenge == component.User.Id)
                {
                    await component.RespondAsync($"{component.User.Mention} has {answer} {MentionUtils.MentionUser(challenger)}s call to duel.");
                    Program.awaitingDuels.Remove(duel);
                    await component.Message.ModifyAsync(x => x.Components = buttons.Build());
                }

                if (component.Data.CustomId.StartsWith("duel_accept"))
                    _ = CountDownInChat(component, challenger, component.User.Id, duel.BetAmount, duelId);
            }
        }

        private async Task PresenceUpdated(SocketUser user, SocketPresence before, SocketPresence after)
        {
            IRole gameRole = null;
            List<SocketGuild> guilds = user.MutualGuilds.ToList();

            if (!before.Activities.Any() && !after.Activities.Any())
                return;

            if (!string.IsNullOrWhiteSpace(before.Activities.FirstOrDefault()?.Name) || !string.IsNullOrWhiteSpace(after.Activities.FirstOrDefault()?.Name))
                if (before.Activities.FirstOrDefault()?.Name != after.Activities.FirstOrDefault()?.Name)
                    await bl.SaveUserAcitivity(user, before.Activities.FirstOrDefault()?.Name ?? "", after.Activities.FirstOrDefault()?.Name ?? "");

            var beforeName = before.Activities.FirstOrDefault()?.Name.Trim() ?? null;
            var afterName = after.Activities.FirstOrDefault()?.Name.Trim() ?? null;

            foreach (var guild in guilds)
            {
                var guildUser = guild.GetUser(user.Id);
                if (after.Activities.FirstOrDefault() != null)
                {
                    if (before.Activities.FirstOrDefault() != null)
                    {
                        if (beforeName == afterName)
                            return;

                        gameRole = guild.Roles.Where(x => x.ToString().Equals(beforeName) && !x.IsMentionable).FirstOrDefault();
                        await DeleteGameRoleAndVoiceChannel(guild, gameRole, guildUser);
                        gameRole = null;
                    }
                    gameRole = guild.Roles.Where(x => x.ToString().Equals(afterName) && !x.IsMentionable).FirstOrDefault();

                    if (gameRole == null) // if role does not exist, create it
                        gameRole = await guild.CreateRoleAsync(afterName, permissions: GuildPermissions.None, color: Color.Default, isHoisted: false, false);
                    var channel = guild.VoiceChannels.Where(x => x.ToString().Equals(gameRole.Name) && x.Bitrate == 96000).FirstOrDefault();
                    if (channel == null)
                    {
                        var properties = new VoiceChannelProperties
                        {
                            Bitrate = 96000
                        };
                        var altChannel = await guild.CreateVoiceChannelAsync(gameRole.Name);
                        await altChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
                        await altChannel.AddPermissionOverwriteAsync(gameRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow));
                        await altChannel.ModifyAsync(x => x.Bitrate = properties.Bitrate);
                    }
                    await guildUser.AddRoleAsync(gameRole);
                }
                else if (before.Activities.FirstOrDefault() != null && after.Activities.FirstOrDefault() == null)
                {
                    gameRole = guild.Roles.Where(x => x.ToString().Equals(beforeName) && !x.IsMentionable).FirstOrDefault();

                    if (gameRole == null)
                        return;

                    await DeleteGameRoleAndVoiceChannel(guild, gameRole, guildUser);
                }
            }
        }

        private Task UserLeft(SocketGuild arg1, SocketUser arg2) => 
            da.DeleteUser(arg2.Id);

        private Task UserJoined(SocketGuildUser arg) => 
            arg.AddRoleAsync(arg.Guild.GetRole(349580645502025728));

        public async Task InitializeAsync() => 
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (rawMessage is not SocketUserMessage message) 
                return;
            if (message.Source != MessageSource.User) 
                return;

            if (Program.serverConfig.blacklistWords.Any(message.Content.ToLower().Contains))
            {
                await message.DeleteAsync();
                return;
            }

            if (Program.serverConfig.whiteList == null || Program.serverConfig.whiteList.All(x => x != message.Author.Id))
            {
                if (!Program.serverConfig.publicChannel.Contains(message.Channel.Id)) // Special channel that does not follow the normal rules
                {
                    if (Program.serverConfig.videoChannel != 0)
                    {
                        if (Program.serverConfig.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id == Program.serverConfig.videoChannel)
                        {
                            if (!freeMsgList.Contains(message.Author))
                                freeMsgList.Add(message.Author);

                            Emoji thumbsUp = new Emoji("👍");
                            await message.AddReactionAsync(thumbsUp);
                        }
                        else if (Program.serverConfig.videoList.Any(message.Content.ToLower().Contains) && message.Channel.Id != Program.serverConfig.videoChannel)
                        {
                            if (!freeMsgList.Contains(message.Author))
                                freeMsgList.Add(message.Author);

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

            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasCharPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                return;
            
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result.Error.HasValue)
                await context.Channel.SendMessageAsync(result.ToString());
        }

        private async Task CountDownInChat(SocketMessageComponent textMessage, ulong challenger, ulong challenge, int betAmount, long duelId)
        {
            var rnd = new Random();
            var originalMsg = await textMessage.GetOriginalResponseAsync();
            for (int i = 3; i > 0; i--)
            {
                var button = new ComponentBuilder().WithButton(i.ToString(), $"duel_countdown{duelId}");
                await textMessage.ModifyOriginalResponseAsync(x => { x.Content = $"{originalMsg.Content}"; x.Components = button.Build(); }); // x.Content = $"{originalMsg.Content}{Environment.NewLine}{i}"
                await Task.Delay(rnd.Next(1001, 1500));
            }

            var duelListItems = new Dictionary<string, string>()
            {
               { "🔫", $"duel_shoot{duelId}" },
               { "🗡", $"duel_dagger{duelId}" },
               { "🔪", $"duel_knife{duelId}" }
            }.Shuffle();

            var finalButton = new ComponentBuilder();

            foreach (var duel in duelListItems)
                finalButton.WithButton(duel.Key, duel.Value);

            await textMessage.ModifyOriginalResponseAsync(x => { x.Content = $"{originalMsg.Content}"; x.Components = finalButton.Build(); });
            Program.activeDuels.Add(new Duel { DuelId = duelId, Challenger = challenger, Challenge = challenge, BetAmount = betAmount, TimeStamp = DateTime.Now });
        }

        public async Task ChangeGameAndRole(Cacheable<SocketGuildUser, ulong> beforeChangeUser, SocketGuildUser afterChangeUser)
        {
            //Console.WriteLine("ChangeGameAndRole");
            SocketGuildUser user = afterChangeUser;
            IRole gameRole = null;
            SocketGuild guild = user.Guild;

            if (!beforeChangeUser.Value.Activities.Any() && !afterChangeUser.Activities.Any())
                return;

            if (!string.IsNullOrWhiteSpace(beforeChangeUser.Value.Activities.FirstOrDefault()?.Name) || !string.IsNullOrWhiteSpace(afterChangeUser.Activities.FirstOrDefault()?.Name))
                if (beforeChangeUser.Value.Activities.FirstOrDefault()?.Name != afterChangeUser.Activities.FirstOrDefault()?.Name)
                    await bl.SaveUserAcitivity(user, beforeChangeUser.Value.Activities.FirstOrDefault()?.Name ?? "", afterChangeUser.Activities.FirstOrDefault()?.Name ?? "");

            var beforeName = beforeChangeUser.Value.Activities.FirstOrDefault()?.Name.Trim() ?? null;
            var afterName = afterChangeUser.Activities.FirstOrDefault()?.Name.Trim() ?? null;

            if (afterChangeUser.Activities.FirstOrDefault() != null)
            {
                if(beforeChangeUser.Value.Activities.FirstOrDefault() != null)
                {
                    if(beforeName == afterName)
                        return;

                    gameRole = guild.Roles.Where(x => x.ToString().Equals(beforeName) && !x.IsMentionable).FirstOrDefault();
                    await DeleteGameRoleAndVoiceChannel(guild, gameRole, user);
                    gameRole = null;
                }
                gameRole = guild.Roles.Where(x => x.ToString().Equals(afterName) && !x.IsMentionable).FirstOrDefault();

                if (gameRole == null) // if role does not exist, create it
                    gameRole = await guild.CreateRoleAsync(afterName, permissions: GuildPermissions.None, color: Color.Default, isHoisted: false, false);
                var channel = guild.VoiceChannels.Where(x => x.ToString().Equals(gameRole.Name) && x.Bitrate == 96000).FirstOrDefault();
                if (channel == null)
                {
                    var properties = new VoiceChannelProperties
                    {
                        Bitrate = 96000
                    };
                    var altChannel = await guild.CreateVoiceChannelAsync(gameRole.Name);
                    await altChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
                    await altChannel.AddPermissionOverwriteAsync(gameRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow));
                    await altChannel.ModifyAsync(x => x.Bitrate = properties.Bitrate); 
                    //await user.ModifyAsync(x => x.Channel = altChannel); // Flyttar användaren
                }
                //else
                //{
                //    await user.ModifyAsync(x => x.Channel = altChannel); // Flyttar användaren
                //}
                await user.AddRoleAsync(gameRole);
            }
            else if (beforeChangeUser.Value.Activities.FirstOrDefault() != null && afterChangeUser.Activities.FirstOrDefault() == null)
            {
                gameRole = guild.Roles.Where(x => x.ToString().Equals(beforeName) && !x.IsMentionable).FirstOrDefault();

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
                    var rng = new Random().Next(0, msges.Count());
                    await guildUser.SendMessageAsync(msges[rng]);
                }
            }
        }

        private System.Timers.Timer aTimer;

        private void SetTimer()
        {
            aTimer = new System.Timers.Timer(Convert.ToInt32(TimeSpan.FromMinutes(Program.serverConfig.BotUpdateTimer).TotalMilliseconds));
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await CelebrateBirthday();
            await GiveGoldToEveryone();
            //Console.WriteLine($"Loop at: {DateTime.Now}");
        }

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

        DateTime lastGoldGiveDateTime = DateTime.MinValue;

        public async Task GiveGoldToEveryone()
        {
            if ((DateTime.Now - lastGoldGiveDateTime).TotalSeconds < 60 * goldToEveryoneTimer)
                return;

            lastGoldGiveDateTime = DateTime.Now;

            var guilds = _discord.Guilds;
            foreach (var guild in guilds)
            {
                var onlineUsers = guild.Users.Where(x => !x.IsSelfDeafened && x.Status == UserStatus.Online && !x.IsBot).ToList();
                onlineUsers.Remove(bl.GetCurrentRichestPerson(guild));
                var userActivities = onlineUsers.GroupBy(x => new { x.Activities.FirstOrDefault()?.Name })
                    .Where(x => x.Key.Name != null && x.Count() > 1 && x.Key.Name != "Custom Status")
                    .Select(x => x.ToList());

                foreach (var user in onlineUsers)
                    await bl.SaveGold(user, guild, 1);

                foreach (var act in userActivities)
                    foreach (var user in act)
                        await bl.SaveGold(user, guild, 2);
            }
            //await da.GiveGoldEveryone(users, 1); // Is super wierd after a days running for some reason
            //await da.GiveGoldEveryone(extraGoldUsers, 2);
        }
    }
}
