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
        int winningNumber = 56;
        DataAccess da;
        MarvBotBusinessLayer bl;

        public CurrencyCommands()
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);
        }

        [Command("How")]
        [Alias("help", "wat", "what")]
        public async Task HowGold()
        {
            await ReplyAsync($"You can earn gold by being online. Every 10 minutes 1 gold is given to everyone who is **online**, and 2 extra gold to everyone who is playing a game together.{Environment.NewLine}" +
                $"You can gamble your gold by typing '!gold roll **Amount**', if you roll a {winningNumber} or above, you win, else you lose. You can also duel people and bet gold on your self by typing '!duel **@User** **Amount**.'");
        }

        [Command("Me")]
        [Alias("", "my", "stash")]
        public async Task MeGold()
        {
            var userGold = await da.GetGold(Context.User.Id);
            await ReplyAsync($"You have **{(userGold).ToString("n0", nfi)}** gold.");
        }

        [Command("info")]
        [Alias("howmuch", "bank", "stash")]
        public async Task InfoGold(IUser user)
        {
            var userGold = await da.GetGold(user.Id);
            await ReplyAsync($"{user.Mention} has **{userGold.ToString("n0", nfi)}** gold.");
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
                var reply = await Gamble(amount);
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
                reply += await Gamble(amount);

            await ReplyAsync(reply);
        }

        private async Task<string> Gamble(int betAmount) // Should be in business layer
        {
            string reply = "";
            var currentGold = await da.GetGold(Context.User.Id);

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
            if (result == 100 && betAmount >= jackpotBorder)
                if (rng.Next(0, 101) != 100)
                    result = 99;

            var cheatList = Program.serverConfig.whiteList;
            if (cheatList.Contains(Context.User.Id)) // cheat
                result = rng.Next(winningNumber, 100);

            var nextRoll = await da.GetNextRoll(Context.User.Id, true);

            if (nextRoll != null)
                result = nextRoll.NextRoll;
           
            reply += $"You rolled {result}." + Environment.NewLine;
            bool won;

            var changeAmount = betAmount;
            if (result >= winningNumber)
            {
                int jackpot = await da.GetGold(276456075559960576);
                won = true;
                if (result == 100 && betAmount < jackpot && betAmount >= jackpotBorder)
                {
                    changeAmount = jackpot;
                    reply += ($":tada: {Context.User.Mention} **WIN THE JACKPOT**, **{jackpot.ToString("n0", nfi)}** gold has been added to your bank. :tada:") + Environment.NewLine;
                    await da.SaveGoldToBot(-jackpot + jackpotBorder);
                }
                else
                {
                    reply += ($"{Context.User.Mention} **WIN**, **{changeAmount.ToString("n0", nfi)}** gold has been added to your bank.") + Environment.NewLine;
                }
                await bl.SaveGold(Context.User, Context.Guild, changeAmount);
            }
            else
            {
                won = false;
                reply += ($"{Context.User.Mention} has lost, **{betAmount.ToString("n0", nfi)}** gold has been removed from your bank.") + Environment.NewLine;
                await bl.SaveGold(Context.User, Context.Guild, -betAmount);
                if (betAmount > 1)
                    await da.SaveGoldToBot(betAmount / 2);
            }
            await da.UpdateGambleAmount(Context.User);
            await da.SaveStats(Context.User, won, betAmount, changeAmount, result);

            return reply;
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
                await ReplyAsync($"You only have {currentGold.ToString("n0", nfi)}. Can't give more than you have.");
                return;
            }

            await bl.SaveGold(Context.User, Context.Guild, -amount);
            await ReplyAsync($"{Context.User.Mention} has just given {user.Mention} **{amount.ToString("n0", nfi)}** gold.");
            await bl.SaveGold(user, Context.Guild, amount);
        }

        [RequireOwner]
        [Command("Take")]
        [Alias("steal", "ta")]
        public async Task TakeGold(IUser user, int amount = 1)
        {
            await ReplyAsync($"{Context.User.Mention} has just taken **{amount.ToString("n0", nfi)}** gold from {user.Mention}.");
            await bl.SaveGold(user, Context.Guild, -amount);
        }

        [RequireOwner]
        [Command("giveEveryone")]
        public async Task GiveGoldEveryone(int amount)
        {
            var users = Context.Guild.Users;
            await da.GiveGoldEveryone(users.Where(x => !x.IsSelfDeafened && x.Status == UserStatus.Online && !x.IsBot).ToList(), amount);
            await ReplyAsync($"You have given everyone who is online, **{amount.ToString("n0", nfi)}** gold");
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
                reply += $"{i}: {MentionUtils.MentionUser(top.UserID)} has **{top.GoldAmount.ToString("n0", nfi)}** gold" + Environment.NewLine;
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
            if (user == null)
                user = Context.User;

            var stats = await da.GetStats(user.Id);

            if (stats == null)
            {
                await ReplyAsync($"Can't find any stats on {user.Mention}.");
                return;
            }

            var reply = "";
            reply += ($"{user.Mention} has gambled **{stats.Count()}** time(s)") + Environment.NewLine;
            float winPercent = ((float)stats.Where(x => x.Won == true).Count() / (float)stats.Count()) * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their gambles") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Won == true).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", nfi)}** gold") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Won == false).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", nfi)}** gold") + Environment.NewLine;
            await ReplyAsync(reply);
        }
        
        [Command("Today")]
        [Alias("Todaystats", "statstoday"), Summary("Gets stats for **user** from the last 24 hours")]
        public async Task GetStatsToday(IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var yesterday = DateTime.Now.AddDays(-1);
            var stats = await da.GetStats(user.Id, yesterday);

            if (stats == null)
            {
                await ReplyAsync($"Can't find any stats on {user.Mention} in the last 24 hours.");
                return;
            }

            var reply = "";
            reply += ($"{user.Mention} has gambled **{stats.Count()}** time(s) in the last 24 hours.") + Environment.NewLine;
            float winPercent = ((float)stats.Where(x => x.Won == true).Count() / (float)stats.Count()) * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their gambles in the last 24 hours") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Won == true).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", nfi)}** gold in the last 24 hours") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Won == false).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", nfi)}** gold in the last 24 hours") + Environment.NewLine;
            await ReplyAsync(reply);
        }

        [Command("freeGold")]
        [Alias("pls", "plsSir", "välfärd"), Summary("Gives you a random amount of gold between 1-100")]
        public async Task DailyFreeGold()
        {
            var top3 = await da.GetTopXGold(3).Pipe(x => x.Select(y => y.UserID).ToList());
            if (top3.Contains(Context.User.Id))
            {
                await ReplyAsync("You are too rich to recieve free cash.");
                return;
            }

            var lastDonation = await da.GetLatestDonation(Context.Guild.Id);
            int waitHours = Program.serverConfig.donationWaitHours;

            if (lastDonation != null && (DateTime.Now - lastDonation.TimeStamp).TotalHours < waitHours)
            {
                await ReplyAsync($"Last donations was given to {MentionUtils.MentionUser(lastDonation.UserID)} at {lastDonation.TimeStamp:yyyy-MM-dd HH:mm:ss}. I only give out 1 donation per {waitHours} hours");
                return;
            }

            var botGoldAmount = await da.GetGold(276456075559960576);

            if (botGoldAmount <= 0)
            {
                await ReplyAsync("I have no money left to give.");
                return;
            }

            var rng = new Random();
            var donationAmount = rng.Next(1, 101);

            if (donationAmount > botGoldAmount)
                donationAmount = botGoldAmount;

            await da.SaveGoldToBot(-donationAmount);
            await bl.SaveGold(Context.User, Context.Guild, donationAmount);
            await da.SetDonation(Context.User, Context.Guild.Id, donationAmount);
            await ReplyAsync($"I have given you **{donationAmount}** gold. Spend with care.");
        }

        [Command("Purge")]
        [Alias("delete", "remove", "annihilate", "kill"), Summary("Deletes **user's** **amount** gold")]
        public async Task GoldPurge(IUser user, int amount = 0)
        {
            var richBitch = Context.Guild.Users.First(x => x.Roles.Select(z => z.Id).ToList().Contains(762789255965048833));
            if (richBitch.Id != Context.User.Id)
            {
                await ReplyAsync($"Only {MentionUtils.MentionRole(762789255965048833)} can purge someone");
                return;
            }
            if (user == null)
            {
                await ReplyAsync("Type '!Gold purge *User* *Amount*'");
                return;
            }
            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Can't purge yourself");
                return;
            }
            var usersGold = await da.GetGold(user.Id);
            if (amount == 0 || amount > usersGold)
                amount = usersGold;
            if (amount <= 0)
            {
                await ReplyAsync($"{user.Mention} has too little money");
                return;
            }

            var richestGold = await da.GetGold(Context.User.Id);

            if (richestGold < amount)
            {
                await ReplyAsync($"You need more gold than **{amount}** to purge {user.Mention}");
                return;
            }

            await ReplyAsync($"{Context.User.Mention} has removed **{amount}** gold from {user.Mention}");
            await bl.SaveGold(Context.User, Context.Guild, -amount);
            await bl.SaveGold(user, Context.Guild, -amount);
        }
    }
}
