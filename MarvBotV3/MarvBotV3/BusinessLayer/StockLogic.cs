﻿using Discord;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.BusinessLayer
{
    public class StockLogic
    {
        DataAccess _da;
        MarvBotBusinessLayer _bl;

        public StockLogic(DataAccess da, MarvBotBusinessLayer bl)
        {
            _da = da;
            _bl = bl;
        }

        public async Task<string> Invest(int InvestAmount, IUser user, IGuild guild) // Should be in business layer
        {
            var currentGold = await _da.GetGold(user.Id);

            if (currentGold < InvestAmount)
                return($"You only have {currentGold.ToString("n0", Program.nfi)}. Can't Invest more.");
            if (InvestAmount <= 0)
                return($"You can't Invest 0 gold.");

            await _bl.SaveGold(user, guild, -InvestAmount);
            await _da.SaveInvestment(user, guild.Id, InvestAmount);

            return $"{user.Mention} has invested **{InvestAmount.ToString("n0", Program.nfi)}** gold.";
        }


        public async Task<string> CalculateInvest(IUser user, IGuild guild, int amount)
        {
            var userInvestment = await _da.GetInvestments(user.Id);
            if (userInvestment == null)
            {
                return("You don't have any investments");
            }
            var sum = userInvestment.Sum(x => x.InvestAmount);

            // Change sum with weekly factor
            var amountInvestment = 1.1 * sum;

            if(amount > amountInvestment) 
            {
                return ("You can't sell more money than you have invested");
            }

            await _da.SaveGold(user, guild.Id, amount);
            await _da.SaveInvestment(user, guild.Id, -amount);

            return $"{user.Mention} has sold **{amountInvestment.ToString("n0", Program.nfi)}** of gold in stock.";
        }
    }
}
