﻿using Discord;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.BusinessLayer
{
    public class CurrencyLogic
    {
        int jackpotBorder = 250;
        int winningNumber = 60;
        DataAccess _da;
        MarvBotBusinessLayer _bl;

        public CurrencyLogic(DataAccess da, MarvBotBusinessLayer bl)
        {
            _da = da;
            _bl = bl;
        }

        public async Task<string> Gamble(int betAmount, IUser user, IGuild guild) // Should be in business layer
        {
            string reply = "";
            var currentGold = await _da.GetGold(user.Id);

            if (currentGold < betAmount)
                throw new ArgumentException($"You only have {currentGold.ToString("n0", Program.nfi)}. Can't gamble more.");
            if (betAmount <= 0)
                throw new ArgumentException($"You can't gamble 0 gold.");

            var rng = new Random();
            var result = rng.Next(0, 101);
            if (result == 100 && betAmount >= jackpotBorder)
                if (rng.Next(0, 101) != 100)
                    result = 99;

            if (_bl.GetCurrentRichestPerson(guild).Id == user.Id && result == 100)
                result = rng.Next(winningNumber, 100);

            var cheatList = Program.serverConfig.whiteList;
            if (cheatList.Contains(user.Id)) // cheat
                result = rng.Next(winningNumber, 100);

            var nextRoll = await _da.GetNextRoll(user.Id, true);

            if (nextRoll != null)
                result = nextRoll.NextRoll;

            reply += $"You rolled {result}." + Environment.NewLine;
            bool won;

            var changeAmount = betAmount;
            if (result >= winningNumber)
            {
                int jackpot = await _da.GetGold(276456075559960576);
                won = true;
                if (result == 100 && betAmount < jackpot && betAmount >= jackpotBorder)
                {
                    changeAmount = betAmount * 100 > jackpot ? jackpot : betAmount * 100;
                    reply += ($":tada: {user.Mention} **WIN THE JACKPOT**, **{changeAmount.ToString("n0", Program.nfi)}** gold has been added to your bank. :tada:") + Environment.NewLine;
                    await _da.SaveGoldToBot(-changeAmount + jackpotBorder);
                }
                else
                {
                    reply += ($"{user.Mention} **WIN**, **{changeAmount.ToString("n0", Program.nfi)}** gold has been added to your bank.") + Environment.NewLine;
                }
                await _bl.SaveGold(user, guild, changeAmount);
            }
            else
            {
                won = false;
                reply += ($"{user.Mention} has lost, **{betAmount.ToString("n0", Program.nfi)}** gold has been removed from your bank.") + Environment.NewLine;
                await _bl.SaveGold(user, guild, -betAmount);
                if (betAmount > 1)
                    await _da.SaveGoldToBot(betAmount / 2);
            }
            await _da.UpdateGambleAmount(user);
            await _da.SaveStats(user, won, betAmount, changeAmount, result);

            return reply;
        }

        public async Task<string> GetStats(IUser contextUser, IUser user = null)
        {
            if (user == null)
                user = contextUser;

            var stats = await _da.GetStats(user.Id);
            var userGold = await _da.GetGold(user.Id);

            if (stats == null)
                return $"Can't find any stats on {user.Mention}.";

            var reply = "";
            reply += $"{user.Mention} has **{userGold.ToString("n0", Program.nfi)}** gold." + Environment.NewLine;
            reply += ($"{user.Mention} has gambled **{stats.Count()}** time(s)") + Environment.NewLine;
            float winPercent = ((float)stats.Where(x => x.Won == true).Count() / (float)stats.Count()) * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their gambles") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Won == true).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", Program.nfi)}** gold") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Won == false).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", Program.nfi)}** gold") + Environment.NewLine;
            return reply;
        }

        public async Task<string> GetStatsToday(IUser contextUser, IUser user = null)
        {
            if (user == null)
                user = contextUser;

            var yesterday = DateTime.Now.AddDays(-1);
            var stats = await _da.GetStats(user.Id, yesterday);
            var userGold = await _da.GetGold(user.Id);

            if (stats == null)
                return $"Can't find any stats on {user.Mention} in the last 24 hours.";

            var reply = "";
            reply += $"{user.Mention} has **{userGold.ToString("n0", Program.nfi)}** gold." + Environment.NewLine;
            reply += ($"{user.Mention} has gambled **{stats.Count()}** time(s) in the last 24 hours.") + Environment.NewLine;
            float winPercent = ((float)stats.Where(x => x.Won == true).Count() / (float)stats.Count()) * 100;
            reply += ($"{user.Mention} has **won** {winPercent}% of their gambles in the last 24 hours") + Environment.NewLine;
            var amountWon = stats.Where(x => x.Won == true).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **won** a total amount of **{amountWon.Sum().ToString("n0", Program.nfi)}** gold in the last 24 hours") + Environment.NewLine;
            var amountLost = stats.Where(x => x.Won == false).Select(x => x.ChangeAmount).ToList();
            reply += ($"{user.Mention} has **lost** a total amount of **{amountLost.Sum().ToString("n0", Program.nfi)}** gold in the last 24 hours") + Environment.NewLine;
            return reply;
        }

        public async Task<string> DailyFreeGold(IUser user, IGuild guild)
        {
            var top3 = await _da.GetTopXGold(3).Pipe(x => x.Select(y => y.UserID).ToList());
            if (top3.Contains(user.Id))
                return "You are too rich to recieve free cash.";

            var lastDonation = await _da.GetLatestDonation(guild.Id);
            int waitHours = Program.serverConfig.donationWaitHours;

            if (lastDonation != null && (DateTime.Now - lastDonation.TimeStamp).TotalHours < waitHours)
                return $"Last donations was given to {MentionUtils.MentionUser(lastDonation.UserID)} at {lastDonation.TimeStamp:yyyy-MM-dd HH:mm:ss}. I only give out 1 donation per {waitHours} hours";

            var botGoldAmount = await _da.GetGold(276456075559960576);

            if (botGoldAmount <= 0)
                return "I have no money left to give.";

            var rng = new Random();
            var donationAmount = rng.Next(1, 101);

            if (donationAmount > botGoldAmount)
                donationAmount = botGoldAmount;

            await _da.SaveGoldToBot(-donationAmount);
            await _bl.SaveGold(user, guild, donationAmount);
            await _da.SetDonation(user, guild.Id, donationAmount);
            return $"I have given you **{donationAmount}** gold. Spend with care.";
        }

        public async Task<string> GoldPurge(IUser contextUser, IGuild contextGuild, IUser targetUser, int amount = 0)
        {
            //var richBitch = guild.Users.First(x => x.Roles.Select(z => z.Id).ToList().Contains(richBitchId)); bl.GetCurrentRichestPerson(guild)
            //if (richBitch.Id != contextUser.Id)
            //    return $"Only {MentionUtils.MentionRole(richBitchId)} can purge someone";

            if (contextUser == null)
                return "Type '!Gold purge *User* *Amount*'";

            if (targetUser.Id == contextUser.Id)
                return "Can't purge yourself";

            var usersGold = await _da.GetGold(targetUser.Id);

            if (amount == 0 || amount > usersGold)
                amount = usersGold;

            if (amount <= 0)
                return $"{targetUser.Mention} has too little money";

            var userGoldAmount = await _da.GetGold(contextUser.Id);

            if (userGoldAmount < amount)
                return $"You need more gold than **{amount}** to purge {targetUser.Mention}";

            await _bl.SaveGold(contextUser, contextGuild, -amount);
            await _bl.SaveGold(targetUser, contextGuild, -amount);
            return $"{contextUser.Mention} has removed **{amount}** gold from {targetUser.Mention}";
        }
    }
}
