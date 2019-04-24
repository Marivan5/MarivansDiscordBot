using Discord;
using Discord.Commands;
using MarvBotV3.Database;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("Gold")]
    [Alias("cash", "dinero", "money", "currency")]
    [Summary("Currency group")]
    public class CurrencyCommands : ModuleBase<ShardedCommandContext>
    {
        [Command("Me")]
        [Alias("my", "stash")]
        public async Task MeGold()
        {
            await ReplyAsync($"You have {DataAccess.GetGold(Context.User.Id).ToString()} gold");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("Give")]
        [Alias("gift", "ge")]
        public async Task GiveGold(IUser user, int amount = 1)
        {
            if(user.IsBot)
            {
                await ReplyAsync("Can't give money to a bot");
                return;
            }

            if(Context.User.Id != Program.serverConfig.serverOwner)
            {
                var currentGold = DataAccess.GetGold(Context.User.Id);
                if(currentGold < amount)
                {
                    await ReplyAsync($"You only have {currentGold}. Can't give more than you have.");
                    return;
                }
                else
                {
                    await DataAccess.SaveGold(Context.User.Id, -amount);
                }
            }

            await ReplyAsync($"{Context.User.Mention} has just given {user.Mention} **{amount}** gold");

            await DataAccess.SaveGold(user.Id, amount);
        }
        
    }
}
