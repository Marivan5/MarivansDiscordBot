﻿using Discord;
using Discord.Commands;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("RockPaperScissors")]
    [Alias("RPS", "StenSaxPåse", "ssp")]
    [Summary("RPS group")]
    public class RockPaperScissorsCommands : ModuleBase<CommandContext>
    {
        DataAccess da;

        public RockPaperScissorsCommands()
        {
            da = new DataAccess(new DatabaseContext());
        }

        [Command("Stats")]
        [Alias("info", "stat")]
        public async Task GetStats(IUser user = null)
        {
            if (user == null)
                user = Context.User;

            var reply = "";
            var stats = await da.GetRockPaperScissorsStats(user.Id);

            if (stats == null)
            {
                await ReplyAsync($"Can't find any stats on {user.Mention}.");
                return;
            }

            float winPercent = (float)stats.Where(x => x.Winner == user.Id).Count() / stats.Count * 100;
            reply += $"{user.Mention} has **won** {winPercent}% of their rock paper scissors games, from a total of {stats.Count} challenges." + Environment.NewLine;
            var amountWon = stats.Where(x => x.Winner == user.Id).Select(x => x.BetAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", Program.nfi)}** gold from challenges") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Winner != user.Id).Select(x => x.BetAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", Program.nfi)}** gold from challenges") + Environment.NewLine;
            var amountChallenged = stats.Where(x => x.Challenger == user.Id).Count();
            reply += ($"{user.Mention} has challenged another user {amountChallenged} times. While they have been challenged by other users {stats.Count - amountChallenged} times.") + Environment.NewLine;
            await ReplyAsync(reply);
        }

        [Command("RockPaperScissors")]
        [Alias("RPS", "StenSaxPåse", "ssp", "")]
        public async Task RpsAmount(IUser challenge, int amount = 0)
        {
            await RockPaperScissors(challenge, amount);
        }

        [Command("RockPaperScissors")]
        [Alias("RPS", "StenSaxPåse", "ssp", "")]
        public async Task RpsAmount(IUser challenge, string input)
        {
            if (input.ToLower() == "all" || input.ToLower() == "allt")
            {
                var user1Gold = await da.GetGold(Context.User.Id);
                var user2Gold = await da.GetGold(challenge.Id);
                var amount = Math.Min(user1Gold, user2Gold);
                await RockPaperScissors(challenge, amount);
            }
            else
            {
                await ReplyAsync("Type **!RockPaperScissors @User all** if you want to bet it all");
            }
        }

        private async Task RockPaperScissors(IUser challenge, int amount = 0)
        {
            if (challenge.Id == 276456075559960576)
            {
                await ReplyAsync($"Save your gold. You can't win over me.");
                return;
            }

            var challengerGold = await da.GetGold(Context.User.Id);
            var challengeGold = await da.GetGold(challenge.Id);

            if (challengerGold < amount)
            {
                await ReplyAsync($"You only have {challengerGold.ToString("n0", Program.nfi)} gold.");
                return;
            }
            else if (challengeGold < amount)
            {
                await ReplyAsync($"{challenge.Mention} only has {challengeGold.ToString("n0", Program.nfi)} gold.");
                return;
            }
            if (challenge == Context.User)
            {
                await ReplyAsync("You can't challenge yourself.");
                return;
            }

            var id = DateTime.Now.Ticks;

            var buttons = new ComponentBuilder().WithButton("🥌", $"rps_rock{id}").WithButton("🧻", $"rps_paper{id}").WithButton("✂", $"rps_scissors{id}").WithButton("Decline", $"rps_decline{id}");
            Program.activeRockPaperScissorsEvents.Add(new Dto.RockPaperScissors { Id = id, Challenger = Context.User.Id, Challenge = challenge.Id, BetAmount = amount, TimeStamp = DateTime.Now });
            await ReplyAsync($"{Context.User.Mention} has challenged {challenge.Mention} to Rock Paper Scissors for {amount.ToString("n0", Program.nfi)} gold{Environment.NewLine}{challenge.Mention} do you accept? ", components: buttons.Build());
        }
    }
}
