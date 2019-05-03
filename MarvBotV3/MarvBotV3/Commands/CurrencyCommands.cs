using Discord;
using Discord.Commands;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("Gold")]
    [Alias("cash", "dinero", "money", "currency", "bank", "ducates", "euro", "dollar", "dollaroos")]
    [Summary("Currency group")]
    public class CurrencyCommands : ModuleBase<SocketCommandContext>
    {
        int jackpotBorder = 250;
        int maxGamblesPer10Min = 2;

        [Command("Me")]
        [Alias("", "my", "stash")]
        public async Task MeGold()
        {
            await ReplyAsync($"You have **{DataAccess.GetGold(Context.User.Id).ToString()}** gold.");
        }

        [Command("info")]
        [Alias("howmuch", "bank", "stash")]
        public async Task InfoGold(IUser user)
        {
            await ReplyAsync($"{user.Mention} has **{DataAccess.GetGold(user.Id).ToString()}** gold.");
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
        public async Task GambleGold(int amount = 10)
        {
            var currentGold = DataAccess.GetGold(Context.User.Id);
            var amountOfGambles = DataAccess.GetGambleAmount(Context.User);
            if(amountOfGambles >= maxGamblesPer10Min)
            {
                await ReplyAsync("You have reach your max gambles. Wait 10 minutes and then gamble again.");
                return;
            }
            if (currentGold < amount)
            {
                await ReplyAsync($"You only have {currentGold}. Can't gamble more.");
                return;
            }
            if (amount <= 0)
            {
                await ReplyAsync($"You can't gamble 0 gold.");
                return;
            }

            var rng = new Random();
            var result = rng.Next(0, 101);

            //if(Context.User.Id == Program.serverConfig.serverOwner)
            //{
            //    result = rng.Next(51, 100);
            //}

            var reply = $"You rolled {result}." + Environment.NewLine;

            if(result >= 60)
            {
                int jackpot = DataAccess.GetGold(276456075559960576);
                if (result == 100 && amount < jackpot && amount >= jackpotBorder)
                {
                    await ReplyAsync($"{reply}:tada: You **WIN THE JACKPOT**, **{jackpot.ToString()}** gold has been added to your bank. :tada:");
                    await DataAccess.SaveGold(Context.User, Context.Guild.Id, jackpot);
                    await DataAccess.SaveGoldToBot(-jackpot + jackpotBorder);

                }
                await ReplyAsync($"{reply}You **WIN**, **{amount}** gold has been added to your bank.");
                await DataAccess.SaveGold(Context.User, Context.Guild.Id, amount);
            }
            else
            {
                await ReplyAsync($"{reply}You lose, **{amount}** gold has been removed from your bank.");
                await DataAccess.SaveGold(Context.User, Context.Guild.Id, -amount);
                await DataAccess.SaveGoldToBot(amount);
            }
            await DataAccess.UpdateGambleAmount(Context.User);
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
                if(currentGold < amount)
                {
                    await ReplyAsync($"You only have {currentGold}. Can't give more than you have.");
                    return;
                }
                else
                {
                    await DataAccess.SaveGold(Context.User, Context.Guild.Id, -amount);
                }
            }

            await ReplyAsync($"{Context.User.Mention} has just given {user.Mention} **{amount}** gold.");

            await DataAccess.SaveGold(user, Context.Guild.Id, amount);
        }

        [RequireOwner]
        [Command("Take")]
        [Alias("steal", "ta")]
        public async Task TakeGold(IUser user, int amount = 1)
        {
            if (user.IsBot)
            {
                await ReplyAsync("Can't take money from a bot.");
                return;
            }
            else if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Can't take money from yourself.");
                return;
            }

            await ReplyAsync($"{Context.User.Mention} has just taken **{amount}** gold from {user.Mention}.");

            await DataAccess.SaveGold(user, Context.Guild.Id, -amount);
        }

        [RequireOwner]
        [Command("giveEveryone")]
        public async Task GiveGoldEveryone(int amount)
        {
            var users = Context.Guild.Users;
            await DataAccess.GiveGoldEveryone(users.Where(x => x.Status != UserStatus.Offline).ToList(), amount);
            await ReplyAsync($"You have given @everyone who is online, **{amount}** gold");
        }

        [Command("Toplist")]
        [Alias("top", "richest")]
        public async Task TopGold(int amount = 10)
        {
            var topList = DataAccess.GetAllGold(Context.Guild.Id, amount);
            var reply = "";
            foreach (var top in topList)
            {
                reply += $"{MentionUtils.MentionUser(top.UserID)} has **{top.GoldAmount}** gold" + Environment.NewLine;
            }
            await ReplyAsync(reply);
        }
        
        [Command("Jackpot")]
        public async Task JackpotStash()
        {
            await ReplyAsync($"**{DataAccess.GetGold(276456075559960576).ToString()}** gold is currently in the jackpot. To win the jackpot you have to bet **{jackpotBorder}** gold or more and roll a **100** in a regular gamble.");
        }
    }
}
