using Discord;
using Discord.Interactions;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using RiksbankenService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.SlashCommands;

[Group("bets", "Bets Group")]
public class PollsSlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    DataAccess da;
    MarvBotBusinessLayer bl;

    public PollsSlashCommands()
    {
        da = new DataAccess(new DatabaseContext());
        bl = new MarvBotBusinessLayer(da);
    }

    [SlashCommand("active", "Gets active bets")]
    public async Task GetActivePolls()
    {
        var reply = "";
        var activeBets = await da.GetActiveBets();

        if (!activeBets.Any())
        {
            await RespondAsync("There are no active bets");
            return;
        }

        foreach (var bet in activeBets)
            reply += $"ID: {bet.ID} : {bet.Name}" + Environment.NewLine;

        await RespondAsync(reply);
    }

    [SlashCommand("new", "Creates a new bet")]
    public async Task SetNewPoll(string name)
    {
        name = name.Trim();
        var id = await da.SaveNewPoll(name, Context.User.Id);
        await RespondAsync($"Added new bet with ID: {id}, Name: {name}");
    }

    [SlashCommand("result", "Sets the result of a bet")]
    public async Task GetPollResult(int id, bool result)
    {
        await RespondAsync(await bl.SetResultPoll(id, result, Context.Guild, Context.User.Id));
    }


    [SlashCommand("bet", "Adds a bet to a bet")]
    public async Task BetOnPoll(int id, bool vote, int amount)
    {
        var userAmount = await da.GetGold(Context.User.Id);

        if (userAmount < amount)
        {
            await ReplyAsync("Can't bet more than you have");
            return;
        }
        await bl.SaveGold(Context.User, Context.Guild, -amount);
        await da.SaveNewBet(id, vote, amount, Context.User);
        await RespondAsync($"Added new bet");
    }

    [SlashCommand("bets", "Gets all votes for a bet")]
    public async Task GetVotes(int id)
    {
        var poll = (await da.GetActiveBets()).Where(x => x.ID == id).ToList();

        if (!poll.Any())
        {
            await RespondAsync("Bet not found");
            return;
        }

        var votes = await da.GetActiveBetsOnPoll(id);
        var reply = "";

        foreach (var vote in votes)
            reply += $"Bet prediction: {vote.Bet} - User: {MentionUtils.MentionUser(vote.UserID)} - Bet amount: **{vote.BetAmount.ToString("n0", Program.nfi)}** gold)" + Environment.NewLine;

        if (string.IsNullOrWhiteSpace(reply))
            reply = "No active bets found for bet";

        await RespondAsync(reply);
    }

    [SlashCommand("user-bets", "Gets all bets for a user")]
    public async Task GetVotesForUser(IUser user = null)
    {
        var votes = await bl.GetActiveBets(user);
        var reply = "";

        foreach (var vote in votes)
            reply += $"Bet prediction: {vote.Bet} - User: {MentionUtils.MentionUser(vote.UserID)} - Bet amount: **{vote.BetAmount.ToString("n0", Program.nfi)}** gold)" + Environment.NewLine;

        if (string.IsNullOrWhiteSpace(reply))
            reply = "No active bets found for user";

        await RespondAsync(reply);
    }
}

