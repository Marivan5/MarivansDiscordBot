using Discord;
using Discord.Interactions;
using MarvBotV3.BusinessLayer;
using MarvBotV3.Database;
using RiksbankenService;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.SlashCommands;

[Group("Polls", "Polls Group")]
public class PollsSlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    DataAccess da;
    MarvBotBusinessLayer bl;

    public PollsSlashCommands()
    {
        da = new DataAccess(new DatabaseContext());
        bl = new MarvBotBusinessLayer(da);
    }

    [SlashCommand("Active", "Gets active polls")]
    public async Task GetActivePolls()
    {
        var reply = "";
        var activePolls = await da.GetActivePolls();

        if (!activePolls.Any())
        {
            await RespondAsync("There are no active polls");
            return;
        }

        foreach (var poll in activePolls)
            reply += $"ID: {poll.ID} : {poll.Name}";
        await RespondAsync(reply);
    }

    [RequireOwner]
    [SlashCommand("New", "Creates a new poll")]
    public async Task SetNewPoll(string name)
    {
        name = name.Trim();
        var id = await da.SaveNewPoll(name, Context.User.Id);
        await RespondAsync($"Added new poll with ID: {id}, Name: {name}");
    }

    [RequireOwner]
    [SlashCommand("Result", "Sets the result of a poll")]
    public async Task GetPollResult(int id, bool result)
    {
        await RespondAsync(await bl.SetResultPoll(id, result, Context.Guild));
    }


    [SlashCommand("Bet", "Adds a bet to a poll")]
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

    [SlashCommand("Bets", "Gets all votes for a poll")]
    public async Task GetVotes(int id)
    {
        var poll = (await da.GetActivePolls()).Where(x => x.ID == id).ToList();

        if (!poll.Any())
        {
            await RespondAsync("Poll not found");
            return;
        }

        var votes = await da.GetActiveBetsOnPoll(id);
        var reply = "";
        foreach (var vote in votes)
            reply += $"Bet prediction: {vote.Bet} - User: {MentionUtils.MentionUser(vote.UserID)} - Bet amount: {vote.BetAmount} gold)";
        await RespondAsync(reply);
    }

    [SlashCommand("User Bets", "Gets all votes for a poll")]
    public async Task GetVotesForUser(IUser user = null)
    {
        var votes = await bl.GetActiveBets(user);
        var reply = "";

        foreach (var vote in votes)
            reply += $"Bet prediction: {vote.Bet} - User: {MentionUtils.MentionUser(vote.UserID)} - Bet amount: {vote.BetAmount} gold)";

        await RespondAsync(reply);
    }
}

