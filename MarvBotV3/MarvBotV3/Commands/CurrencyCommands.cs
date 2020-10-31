using Discord;
using Discord.Commands;
using MarvBotV3.Database;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("Gold")]
    [Alias("cash", "dinero", "money", "currency", "bank", "ducates", "euro", "dollar", "dollaroos")]
    [Summary("Currency group")]
    public class CurrencyCommands : ModuleBase<SocketCommandContext>
    {
        private readonly NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " " };
        int jackpotBorder = 250;

        [Command("Me")]
        [Alias("", "my", "stash")]
        public async Task MeGold()
        {
            await ReplyAsync($"You have **{DataAccess.GetGold(Context.User.Id).ToString("n0", nfi)}** gold.");
        }

        [Command("info")]
        [Alias("howmuch", "bank", "stash")]
        public async Task InfoGold(IUser user)
        {
            await ReplyAsync($"{user.Mention} has **{DataAccess.GetGold(user.Id).ToString("n0", nfi)}** gold.");
        }

        [RequireOwner]
        [Command("delete")]
        [Alias("remove", "kill")]
        public async Task DeleteUser(ulong userID)
        {
            await DataAccess.DeleteUser(userID);
            await ReplyAsync($"Removed all of {MentionUtils.MentionUser(userID)}'s gold");
        }

        [Command("Gamble")]
        [Alias("roll", "dice")]
        public async Task GambleGold(string input)
        {
            if (input.ToLower() == "all" || input.ToLower() == "all in")
            {
                var amount = DataAccess.GetGold(Context.User.Id);
                var reply = await Gamble(amount);
                await ReplyAsync(reply);
            }
            else
            {
                await ReplyAsync("Type **!gold roll all** if you want to gamble it all");
            }
        }

        [Command("Gamble")]
        [Alias("rolls", "dices", "roll", "dice")]
        public async Task GamblesGold(int amount = 10, int times = 1)
        {
            if (times > 20)
                times = 20;

            string reply = "";

            for (int i = 0; i < times; ++i)
            {
                reply += await Gamble(amount);
            }
            await ReplyAsync(reply);
        }

        private async Task<string> Gamble(int betAmount)
        {
            string reply = "";
            var currentGold = DataAccess.GetGold(Context.User.Id);
            //var amountOfGambles = DataAccess.GetGambleAmount(Context.User);
            //if (amountOfGambles >= Program.maxGambles)
            //{
            //    reply += "You have reach your max gambles. Wait 10 minutes and then gamble again.";
            //    return reply;
            //}
            if (currentGold < betAmount)
            {
                reply += $"You only have {currentGold.ToString("n0", nfi)}. Can't gamble more.";
                return reply;
            }
            if (betAmount <= 0)
            {
                reply += $"You can't gamble 0 gold.";
                return reply;
            }

            var rng = new Random();
            var result = rng.Next(0, 101);
            var cheatList = Program.serverConfig.whiteList;
            if (cheatList.Contains(Context.User.Id)) // cheat
                result = rng.Next(60, 100);
            if (Program.nextRolls.Any())
            {
                result = Program.nextRolls.First();
                Program.nextRolls.Remove(result);
            }

            reply += $"You rolled {result}." + Environment.NewLine;
            bool won;

            var changeAmount = betAmount;
            if (result >= 60)
            {
                int jackpot = DataAccess.GetGold(276456075559960576);
                won = true;
                if (result == 100 && betAmount < jackpot && betAmount >= jackpotBorder)
                {
                    changeAmount = jackpot;
                    reply += ($":tada: {Context.User.Mention} **WIN THE JACKPOT**, **{jackpot.ToString("n0", nfi)}** gold has been added to your bank. :tada:") + Environment.NewLine;
                    await DataAccess.SaveGoldToBot(-jackpot + jackpotBorder);
                }
                else
                {
                    reply += ($"{Context.User.Mention} **WIN**, **{changeAmount.ToString("n0", nfi)}** gold has been added to your bank.") + Environment.NewLine;
                }
                await BusinessLayer.SaveGold(Context.User, Context.Guild, changeAmount);
            }
            else
            {
                won = false;
                reply += ($"{Context.User.Mention} has lost, **{betAmount.ToString("n0", nfi)}** gold has been removed from your bank.") + Environment.NewLine;
                await BusinessLayer.SaveGold(Context.User, Context.Guild, -betAmount);
                if (betAmount > 1)
                {
                    await DataAccess.SaveGoldToBot(betAmount / 2);
                }
            }
            await DataAccess.UpdateGambleAmount(Context.User);
            await DataAccess.SaveStats(Context.User, won, betAmount, changeAmount, result);

            return reply;
        }

        [Command("Give")]
        [Alias("gift", "ge")]
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

            if (Context.User.Id != Program.serverConfig.serverOwner)
            {
                var currentGold = DataAccess.GetGold(Context.User.Id);
                if (currentGold < amount)
                {
                    await ReplyAsync($"You only have {currentGold.ToString("n0", nfi)}. Can't give more than you have.");
                    return;
                }
                else
                {
                    await BusinessLayer.SaveGold(Context.User, Context.Guild, -amount);
                }
            }

            await ReplyAsync($"{Context.User.Mention} has just given {user.Mention} **{amount.ToString("n0", nfi)}** gold.");
            await BusinessLayer.SaveGold(user, Context.Guild, amount);
        }

        [RequireOwner]
        [Command("Take")]
        [Alias("steal", "ta")]
        public async Task TakeGold(IUser user, int amount = 1)
        {
            await ReplyAsync($"{Context.User.Mention} has just taken **{amount.ToString("n0", nfi)}** gold from {user.Mention}.");
            await BusinessLayer.SaveGold(user, Context.Guild, -amount);
        }

        [RequireOwner]
        [Command("giveEveryone")]
        public async Task GiveGoldEveryone(int amount)
        {
            var users = Context.Guild.Users;
            await DataAccess.GiveGoldEveryone(users.Where(x => !x.IsSelfDeafened && x.Status == UserStatus.Online && !x.IsBot).ToList(), amount);
            await ReplyAsync($"You have given everyone who is online, **{amount.ToString("n0", nfi)}** gold");
        }

        [Command("Toplist")]
        [Alias("top", "richest")]
        public async Task TopGold(int amount = 10)
        {
            var topList = DataAccess.GetTopXGold(amount);
            var reply = "";
            foreach (var top in topList)
            {
                reply += $"{MentionUtils.MentionUser(top.UserID)} has **{top.GoldAmount.ToString("n0", nfi)}** gold" + Environment.NewLine;
            }
            await ReplyAsync(reply);
        }

        [Command("Jackpot")]
        public async Task JackpotStash()
        {
            await ReplyAsync($"**{DataAccess.GetGold(276456075559960576)}** gold is currently in the jackpot. To win the jackpot you have to bet **{jackpotBorder}** gold or more and roll a **100** in a regular gamble.");
        }

        [Command("Stats")]
        [Alias("info", "stat")]
        public async Task GetStats(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            var reply = "";
            var stats = DataAccess.GetStats(user.Id);

            if (stats == null)
            {
                await ReplyAsync($"Can't find any stats on {user.Mention}.");
                return;
            }

            float winPercent = ((float)stats.Where(x => x.Won == true).Count() / (float)stats.Count()) * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their gambles.") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Won == true).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", nfi)}** gold") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Won == false).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", nfi)}** gold") + Environment.NewLine;
            await ReplyAsync(reply);
        }

        //[RequireOwner]
        //[Command("MaxGambles")]
        //public async Task SetMaxGambles(int amount)
        //{
        //    Program.serverConfig.maxGambles = amount;
        //    Program.maxGambles = amount;
        //    Program.serverConfig.Save();
        //    await ReplyAsync($"Max gambles have been set to {amount.ToString("n0", nfi)}.");
        //}
    }
}
