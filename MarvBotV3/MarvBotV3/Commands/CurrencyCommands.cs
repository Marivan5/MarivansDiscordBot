﻿using Discord;
using Discord.Commands;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("Gold")]
    [Alias("cash", "dinero", "money", "currency", "bank", "ducates", "euro", "dollar", "dollaroos")]
    [Summary("Currency group")]
    public class CurrencyCommands : ModuleBase<CommandContext>
    {
        int jackpotBorder = 250;
        int winningNumber = 60;
        DataAccess da;
        MarvBotBusinessLayer bl;
        CurrencyLogic cl;

        public CurrencyCommands()
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);
            cl = new CurrencyLogic(da, bl);
        }

        [Command("How")]
        [Alias("help", "wat", "what")]
        public async Task HowGold()
        {
            await ReplyAsync($"You can earn gold by being online. Every 10 minutes 1 gold is given to everyone who is **online**, and 2 extra gold to everyone who is playing a game together.{Environment.NewLine}" +
                $"You can gamble your gold by typing '!gold roll **Amount**', if you roll a {winningNumber} or above, you win, else you lose. You can also duel people and bet gold on your self by typing '!duel **@User** **Amount**.'");
        }

        [Command("info")]
        [Alias("", "howmuch", "bank", "stash")]
        public async Task InfoGold(IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var userGold = await da.GetGold(user.Id);
            await ReplyAsync($"{user.Mention} has **{userGold.ToString("n0", Program.nfi)}** gold.");
        }

        [RequireOwner]
        [Command("delete")]
        [Alias("remove", "kill")]
        public async Task DeleteUser(ulong userID)
        {
            await da.DeleteUser(userID);
            await ReplyAsync($"Removed all of {MentionUtils.MentionUser(userID)}'s gold");
        }

        [Command("Gamble"), Summary("Rolls a random number between to win the same amount you have")]
        [Alias("roll", "dice")]
        public async Task GambleGold(string input)
        {
            if (input.ToLower() == "all" || input.ToLower() == "all in")
            {
                var amount = await da.GetGold(Context.User.Id);
                string reply;
                try
                {
                    reply = await cl.Gamble(amount, Context.User, Context.Guild);
                }
                catch (Exception e)
                {
                    reply = e.Message;
                }

                await ReplyAsync(reply);
            }
            else
            {
                await ReplyAsync("Type **!gold roll all** if you want to gamble it all");
            }
        }

        [Command("Roll"), Summary("Rolls a random number to win the amount you bet")]
        [Alias("rolls", "dices", "gamble", "dice")]
        public async Task GamblesGold(int amount = 10, int times = 1)
        {
            if (times > 20)
                times = 20;

            string reply = "";

            for (int i = 0; i < times; ++i)
                try
                {
                    reply += await cl.Gamble(amount, Context.User, Context.Guild);
                }
                catch (Exception e)
                {
                    reply += e.Message;
                    break;
                }

            await ReplyAsync(reply);
        }

        [Command("Give")]
        [Alias("gift", "ge"), Summary("Gives **user** an set **amount** of gold")]
        public async Task GiveGold(IUser user, int amount = 1)
        {
            if (user.IsBot)
            {
                await ReplyAsync("Can't give money to a bot.");
                return;
            }
            else if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Can't give yourself money.");
                return;
            }

            var currentGold = await da.GetGold(Context.User.Id);
            if (currentGold < amount)
            {
                await ReplyAsync($"You only have {currentGold.ToString("n0", Program.nfi)}. Can't give more than you have.");
                return;
            }

            await bl.SaveGold(Context.User, Context.Guild, -amount);
            await ReplyAsync($"{Context.User.Mention} has just given {user.Mention} **{amount.ToString("n0", Program.nfi)}** gold.");
            await bl.SaveGold(user, Context.Guild, amount);
        }

        [RequireOwner]
        [Command("Take")]
        [Alias("steal", "ta")]
        public async Task TakeGold(IUser user, int amount = 1)
        {
            await ReplyAsync($"{Context.User.Mention} has just taken **{amount.ToString("n0", Program.nfi)}** gold from {user.Mention}.");
            await bl.SaveGold(user, Context.Guild, -amount);
        }

        [RequireOwner]
        [Command("giveEveryone")]
        public async Task GiveGoldEveryone(int amount)
        {
            var users = await Context.Guild.GetUsersAsync();
            await da.GiveGoldEveryone(users.Where(x => !x.IsSelfDeafened && x.Status == UserStatus.Online && !x.IsBot).ToList(), amount);
            await ReplyAsync($"You have given everyone who is online, **{amount.ToString("n0", Program.nfi)}** gold");
        }

        [Command("Toplist")]
        [Alias("top", "richest"), Summary("Lists out the richest users")]
        public async Task TopGold(int amount = 10)
        {
            var topList = await da.GetTopXGold(amount);
            var reply = "";
            var i = 1;
            foreach (var top in topList)
            {
                reply += $"{i}: {MentionUtils.MentionUser(top.UserID)} has **{top.GoldAmount.ToString("n0", Program.nfi)}** gold" + Environment.NewLine;
                i++;
            }
            await ReplyAsync(reply);
        }

        [Command("Jackpot"), Summary("Displays what the jackpot is at")]
        public async Task JackpotStash() => 
            await ReplyAsync($"**{await da.GetGold(276456075559960576)}** gold is currently in the jackpot. To win the jackpot you have to bet **{jackpotBorder}** gold or more and roll a **100** in a regular gamble.");

        [Command("Stats")]
        [Alias("info", "stat"), Summary("Gets stats for **user**")]
        public async Task GetStats(IUser user = null)
        {
            await Context.Message.ReplyAsync(await cl.GetStats(Context.User, user));
        }
        
        [Command("Today")]
        [Alias("Todaystats", "statstoday"), Summary("Gets stats for **user** from the last 24 hours")]
        public async Task GetStatsToday(IUser user = null)
        {
            await Context.Message.ReplyAsync(await cl.GetStatsToday(Context.User, user));
        }

        [Command("freeGold")]
        [Alias("pls", "plsSir", "välfärd"), Summary("Gives you a random amount of gold between 1-100")]
        public async Task DailyFreeGold()
        {
            await Context.Message.ReplyAsync(await cl.DailyFreeGold(Context.User, Context.Guild));
        }

        [Command("Purge")]
        [Alias("delete", "remove", "annihilate", "kill"), Summary("Deletes **user's** **amount** gold")]
        public async Task GoldPurge(IUser user, int amount = 0)
        {
            await Context.Message.ReplyAsync(await cl.GoldPurge(Context.User, Context.Guild, user, amount));
        }
    }
}
