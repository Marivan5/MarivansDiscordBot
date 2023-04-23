using Discord.Interactions;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using System;
using System.Threading.Tasks;

namespace MarvBotV3.SlashCommands
{
    [Group("invest", "Invest Group")]
    public class StockSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        DataAccess da;
        MarvBotBusinessLayer bl;
        StockLogic sl;

        public StockSlashCommands()
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);
            sl = new StockLogic(da, bl);
        }

        [RequireOwner]
        [SlashCommand("buy", "invest in stock")]
        public async Task InvestGold(int investment = 10)
        {
            string reply = "";

            try
            {
                reply += await sl.Invest(investment, Context.User, Context.Guild);
            }
            catch (Exception e)
            {
                reply += e.Message;
            }

            await RespondAsync(reply);
        }

        [RequireOwner]
        [SlashCommand("sell", "invest in stock")]
        public async Task SellInvestment(int amount)
        {
            string reply = "";

            try
            {
                reply += await sl.CalculateInvest(Context.User, Context.Guild, amount);
            }
            catch (Exception e)
            {
                reply += e.Message;
            }

            await RespondAsync(reply);
        }
    }
}
