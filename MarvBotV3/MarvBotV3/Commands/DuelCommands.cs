using Discord;
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
    public class DuelCommands : ModuleBase<SocketCommandContext>
    {
        private NumberFormatInfo nfi = new NumberFormatInfo { NumberGroupSeparator = " " };

        [Command("Stats")]
        [Alias("info", "stat")]
        public async Task GetStats(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            var reply = "";
            var stats = DataAccess.GetDuelStats(user.Id);

            if (stats == null)
            {
                await ReplyAsync($"Can't find any stats on {user.Mention}.");
                return;
            }

            float winPercent = ((float)stats.Where(x => x.Winner == user.Id).Count() / (float)stats.Count()) * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their duels.") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Winner == user.Id).Select(x => x.BetAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", nfi)}** gold from duels") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Winner != user.Id).Select(x => x.BetAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", nfi)}** gold from duels") + Environment.NewLine;
            await ReplyAsync(reply);
        }

        [Command("Duel")]
        [Alias("fight", "")]
        public async Task Duel(IUser challenge, int amount = 0)
        {
            if (challenge.Id == 276456075559960576)
            {
                await ReplyAsync($"yes{Environment.NewLine}3{Environment.NewLine}2{Environment.NewLine}1{Environment.NewLine}Shoot!{Environment.NewLine}🔫{Environment.NewLine}I WIN!");
                return;
            }

            var challengerGold = DataAccess.GetGold(Context.User.Id);
            var challengeGold = DataAccess.GetGold(challenge.Id);

            if (challengerGold < amount)
            {
                await ReplyAsync($"You only have {challengerGold.ToString("n0", nfi)} gold.");
                return;
            }
            else if (challengeGold < amount)
            {
                await ReplyAsync($"{challenge.Mention} only has {challengeGold.ToString("n0", nfi)} gold.");
                return;
            }
            if (challenge == Context.User)
            {
                await ReplyAsync("You can't duel yourself.");
                return;
            }
            await ReplyAsync($"{Context.User.Mention} has challenged {challenge.Mention} for {amount.ToString("n0", nfi)} gold{Environment.NewLine}{challenge.Mention} do you accept? (type **Yes** to accept)");
            Program.awaitingDuels.Add(new DTO.Duel { Challenger = Context.User.Id, Challenge = challenge.Id, BetAmount = amount, TimeStamp = DateTime.Now });
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
