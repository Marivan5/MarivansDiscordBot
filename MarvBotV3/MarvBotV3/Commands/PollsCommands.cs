using Discord.Commands;
using MarvBotV3.Database;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using System;

namespace MarvBotV3.Commands
{
    [Group("Poll")]
    [Alias("polls")]
    [Summary("Polls")]
    public class PollsCommands : ModuleBase<ShardedCommandContext>
    {
        DataAccess da;
        MarvBotBusinessLayer bl;
        
        public PollsCommands()
        {
            da = new DataAccess(new DatabaseContext());
            bl = new MarvBotBusinessLayer(da);
        }

        [Command("Active")]
        [Alias("Aktiva", "")]
        public async Task Active()
        {
            var reply = "";
            var activePolls = await da.GetActivePolls();

            if (!activePolls.Any())
            {
                await ReplyAsync("There are no active polls");
                return;
            }

            foreach (var poll in activePolls)
                reply += $"ID: {poll.ID} : {poll.Name}";

            await ReplyAsync(reply);
        }

        [RequireOwner]
        [Command("New")]
        [Alias("Ny")]
        public async Task SetNewPoll(params string[] name)
        {
            string pollName = "";
            foreach (var item in name)
                pollName += $"{item} ";

            pollName = pollName.Trim();

            var id = await da.SaveNewPoll(pollName, Context.User.Id);
            await ReplyAsync($"Added new poll with ID: {id}, Name: {pollName}");
        }

        [RequireOwner]
        [Command("Result")]
        [Alias("Res", "Resultat", "Set")]
        public async Task SetResultPoll(long id, bool result) => 
            await ReplyAsync(await bl.SetResultPoll(id, result, Context.Guild));

        [Command("Bet")]
        public async Task BetOnPoll(int id, bool result, int amount)
        {
            var userAmount = await da.GetGold(Context.User.Id);

            if (userAmount < amount)
            {
                await ReplyAsync("Can't bet more than you have");
                return;
            }
            await bl.SaveGold(Context.User, Context.Guild, -amount);
            await da.SaveNewBet(id, result, amount, Context.User);
            await ReplyAsync($"Added new bet");
        }

        [Command("Bets")]
        public async Task GetActiveBets(IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var activeBets = await bl.GetActiveBets(user);

            if (!activeBets.Any())
            {
                await ReplyAsync("You have no active bets");
                return;
            }

            var reply = "";

            foreach (var bet in activeBets)
                reply += $"ID: {bet.PollID}, Amount: {bet.BetAmount}, Prediction: {bet.Bet} {Environment.NewLine}";

            await ReplyAsync(reply);
        }
    }
}
