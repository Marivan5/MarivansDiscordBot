using Discord;
using Discord.Interactions;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.SlashCommands
{
    [Group("gold", "Currency Group")]
    public class CurrencySlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        DataAccess da;
        MarvBotBusinessLayer bl;
        CurrencyLogic cl;

        public CurrencySlashCommands()
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

        //[SlashCommand("", "Gets your gold")]
        //public async Task GetMyGold()
        //{
        //    var userGold = await da.GetGold(Context.User.Id);
        //    await RespondAsync($"You have **{(userGold).ToString("n0", Program.nfi)}** gold.");
        //}
    }
}
