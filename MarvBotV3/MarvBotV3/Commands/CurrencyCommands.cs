using Discord;
using Discord.Commands;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("Gold")]
    [Alias("cash", "dinero", "money", "currency")]
    [Summary("Currency group")]
    public class CurrencyCommands : ModuleBase<SocketCommandContext>
    {
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

        [Command("Gamble")]
        [Alias("roll", "dice")]
        public async Task GambleGold(int amount)
        {
            var currentGold = DataAccess.GetGold(Context.User.Id);
            if (currentGold < amount)
            {
                await ReplyAsync($"You only have {currentGold}. Can't gamble more.");
                return;
            }

            var rng = new Random();
            var result = rng.Next(0, 2);

            var reply = $"You rolled {result}." + Environment.NewLine;

            if(result == 1)
            {
                await ReplyAsync($"{reply}You **WIN**, **{amount}** gold has been added to your bank.");
                await DataAccess.SaveGold(Context.User, amount);
            }
            else
            {
                await ReplyAsync($"{reply}You lose, **{amount}** gold has been removed from your bank.");
                await DataAccess.SaveGold(Context.User, -amount);
            }
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
                    await DataAccess.SaveGold(Context.User, -amount);
                }
            }

            await ReplyAsync($"{Context.User.Mention} has just given {user.Mention} **{amount}** gold.");

            await DataAccess.SaveGold(user, amount);
        }

        [Command("giveEveryone")]
        public async Task GiveGoldEveryone(int amount)
        {
            var users = Context.Guild.Users;
            await DataAccess.GiveGoldEveryone(users.Where(x => x.Status != UserStatus.Offline).ToList(), amount);
            await ReplyAsync($"You have given @everyone who is online, **{amount}** gold");
        }
    }
}
