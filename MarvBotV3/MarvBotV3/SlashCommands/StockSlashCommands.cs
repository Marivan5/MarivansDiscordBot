using Discord;
using Discord.Interactions;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using System;
using System.Threading.Tasks;

namespace MarvBotV3.SlashCommands
{
    [Group("gold", "Currency Group")]
    public class StockSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        DataAccess da;
        MarvBotBusinessLayer bl;
        CurrencyLogic cl;

        public StockSlashCommands()
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);
            cl = new CurrencyLogic(da, bl);
        }

        [SlashCommand("stats", "Gets stats for **user**")]
        public async Task GetStats(IUser user = null)
        {
            await RespondAsync(await cl.GetStats(Context.User, user));
        }

        [SlashCommand("roll-all", "Rolls a random number between 0 and 100, get 60 or above to win the double the amount you have")]
        public async Task GambleGold()
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

            await RespondAsync(reply);
        }

        [SlashCommand("roll", "Rolls a random number between 0 and 100, get 60 or above to win the double the amount you bet")]
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

            await RespondAsync(reply);
        }

        [SlashCommand("Invest", "Invest in stock")]
        public async Task InvestGold(int investment = 10)
        {
            string reply = "";

            try
            {
                reply += await cl.Gamble(investment, Context.User, Context.Guild);
            }
            catch (Exception e)
            {
                reply += e.Message;
            }

            await RespondAsync(reply);
        }

        [SlashCommand("Sell", "Invest in stock")]
        public async Task SellInvestment()
        {
            InvestmentValue = 1.1 * ;

            string reply = "";

            try
            {
                reply += await cl.Gamble(investment, Context.User, Context.Guild);
            }
            catch (Exception e)
            {
                reply += e.Message;
            }

            await RespondAsync(reply);
        }

        [SlashCommand("top-list", "Lists out the richest users")]
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
            await RespondAsync(reply);
        }

        [SlashCommand("pls", "Gives you a random amount of gold between 1-100")]
        public async Task DailyFreeGold()
        {
            await RespondAsync(await cl.DailyFreeGold(Context.User, Context.Guild));
        }

        [SlashCommand("delete", "Deletes **user's** **amount** gold")]
        public async Task GoldPurge(IUser user, int amount = 0)
        {
            await RespondAsync(await cl.GoldPurge(Context.User, Context.Guild, user, amount));
        }
    }
}
