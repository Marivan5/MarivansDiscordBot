﻿using Discord;
using Discord.Commands;
using MarvBotV3.Database;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Commands
{
    [Group("Duel")]
    [Alias("vs", "1v1", "fight")]
    [Summary("Duel group")]
    public class DuelCommands : ModuleBase<ShardedCommandContext>
    {
        private NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " " };
        DataAccess da;

        public DuelCommands()
        {
            da = new DataAccess(new DatabaseContext());
        }

        [Command("Stats")]
        [Alias("info", "stat")]
        public async Task GetStats(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            var reply = "";
            var stats = await da.GetDuelStats(user.Id);

            if (stats == null)
            {
                await ReplyAsync($"Can't find any stats on {user.Mention}.");
                return;
            }

            float winPercent = (float)stats.Where(x => x.Winner == user.Id).Count() / stats.Count() * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their duels.") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Winner == user.Id).Select(x => x.BetAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", nfi)}** gold from duels") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Winner != user.Id).Select(x => x.BetAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", nfi)}** gold from duels") + Environment.NewLine;
            await ReplyAsync(reply);
        }

        [Command("Duel")]
        [Alias("fight", "")]
        public async Task DuelAmount(IUser challenge, int amount = 0)
        {
            var reply = await Duel(challenge, amount);
            await ReplyAsync(reply);
        }

        [Command("Duel")]
        [Alias("fight", "")]
        public async Task DuelAmount(IUser challenge, string input)
        {
            if (input.ToLower() == "all" || input.ToLower() == "all in")
            {
                var user1Gold = await da.GetGold(Context.User.Id);
                var user2Gold = await da.GetGold(challenge.Id);
                var amount = Math.Min(user1Gold, user2Gold);
                var reply = await Duel(challenge, amount);
                await ReplyAsync(reply);
            }
            else
            {
                await ReplyAsync("Type **!duel @User all** if you want to duel it all");
            }
        }

        private async Task<string> Duel(IUser challenge, int amount = 0)
        {
            if (challenge.Id == 276456075559960576)
                return $"yes{Environment.NewLine}3{Environment.NewLine}2{Environment.NewLine}1{Environment.NewLine}Shoot!{Environment.NewLine}🔫{Environment.NewLine}I WIN!";

            var challengerGold = await da.GetGold(Context.User.Id);
            var challengeGold = await da.GetGold(challenge.Id);

            if (challengerGold < amount)
                return $"You only have {challengerGold.ToString("n0", nfi)} gold.";
            else if (challengeGold < amount)
                return $"{challenge.Mention} only has {challengeGold.ToString("n0", nfi)} gold.";
            if (challenge == Context.User)
                return "You can't duel yourself.";

            Program.awaitingDuels.Add(new DTO.Duel { Challenger = Context.User.Id, Challenge = challenge.Id, BetAmount = amount, TimeStamp = DateTime.Now });
            return $"{Context.User.Mention} has challenged {challenge.Mention} for {amount.ToString("n0", nfi)} gold{Environment.NewLine}{challenge.Mention} do you accept? (type **Yes** to accept) (Expires in 1 minute)";
        }

        [Command("Duels")]
        [Alias("fights", "active")]
        public async Task Duels()
        {
            int i = 1;
            if (Program.activeDuels.Count <= 0)
            {
                await ReplyAsync("There are no active duels :(");
                return;
            }

            await ReplyAsync("Current active duels:");

            foreach (var duel in Program.activeDuels)
            {
                await ReplyAsync($"{i}. {MentionUtils.MentionUser(duel.Challenger)} vs {MentionUtils.MentionUser(duel.Challenge)}");
                i++;
            }
        }
        
        [Command("Waiting")]
        [Alias("awaiting", "idle")]
        public async Task Waiting()
        {
            int i = 1;
            if (Program.awaitingDuels.Count <= 0)
            {
                await ReplyAsync("There are no active challanges :(");
                return;
            }

            await ReplyAsync("Current challenges:");

            foreach (var duel in Program.awaitingDuels)
            {
                await ReplyAsync($"{i}. {MentionUtils.MentionUser(duel.Challenger)} vs {MentionUtils.MentionUser(duel.Challenge)}");
                i++;
            }
        }
    }
}
