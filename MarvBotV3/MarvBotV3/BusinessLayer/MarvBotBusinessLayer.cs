using Discord;
using Discord.WebSocket;
using MarvBotV3.Database;
using MarvBotV3.Database.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.BusinessLayer
{
    public class MarvBotBusinessLayer
    {
        private DataAccess _da;

        public MarvBotBusinessLayer(DataAccess da)
        {
            _da = da; 
        }

        public async Task SaveGold(IUser user, IGuild guild, int amount)
        {
            await _da.SaveGold(user, guild.Id, amount);
            await CheckRichestPerson(guild);
        }

        private async Task CheckRichestPerson(IGuild guild)
        {
            var newRichestPerson = (await _da.GetTopXGold(1)).FirstOrDefault()?.UserID ?? 0;

            if (newRichestPerson == 0)
                return;

            var currentRichestPerson = GetCurrentRichestPerson(guild);
            if(currentRichestPerson == null || newRichestPerson != currentRichestPerson.Id)
            {
                var guildRole = (IRole)guild.GetRole(Program.serverConfig.richRole);
                await guild.GetUserAsync(newRichestPerson).Result.AddRoleAsync(guildRole);

                if(currentRichestPerson != null)
                    await currentRichestPerson.RemoveRoleAsync(guildRole);
            }
        }

        public IGuildUser GetCurrentRichestPerson(IGuild guild) => 
            guild.GetUsersAsync().Result.FirstOrDefault(x => x.RoleIds.Contains(Program.serverConfig.richRole));

        public async Task SaveUserAcitivity(IUser user, string beforeActivity, string afterActivity) => 
            await _da.SaveUserAcitivity(user, beforeActivity, afterActivity);

        public int CalculateDaysUntilNextDate(DateTime date, bool calcForward = true)
        {
            DateTime next = date.AddYears(DateTime.Today.Year - date.Year);

            if (next < DateTime.Today && calcForward)
                next = next.AddYears(1);

            return (next - DateTime.Today).Days;
        }

        public string CalculateYourAge(DateTime birthday)
        {
            DateTime Now = DateTime.Now;
            int Years = new DateTime(DateTime.Now.Subtract(birthday).Ticks).Year - 1;
            DateTime PastYearDate = birthday.AddYears(Years);
            int Months = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (PastYearDate.AddMonths(i) == Now)
                {
                    Months = i;
                    break;
                }
                else if (PastYearDate.AddMonths(i) >= Now)
                {
                    Months = i - 1;
                    break;
                }
            }
            int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
            return $"{Years} Year" + (Years > 1 ? "s" : "") + $" {Months} Month" + (Months > 1 ? "s" : "") + $" {Days} Day" + (Days > 1 ? "s" : "");
        }

        public async Task<string> SetResultPoll(long id, bool result, IGuild guild)
        {
            var tbPoll = await _da.GetActivePolls().Pipe(x => x.Where(y => y.ID == id).SingleOrDefault());

            if (tbPoll == null)
                return $"Can't find active poll with ID {id}";

            tbPoll.Result = result;
            tbPoll.ResultTimeStamp = DateTime.Now;

            var tbBets = await _da.GetBetsFromPollId(id);

            foreach (var bet in tbBets)
            {
                var user = await guild.GetUserAsync(bet.UserID);
                var amount = bet.Bet == result ? bet.BetAmount : -bet.BetAmount;
                await SaveGold(user, guild, amount * 2);
            }

            await _da.SaveChanges();

            return $"Result saved for poll: {tbPoll.Name} {Environment.NewLine}" +
                $"Winners: {tbBets.Count(x => x.Bet == result)} {Environment.NewLine}" +
                $"Losers: {tbBets.Count(x => x.Bet != result)}";
        }

        public async Task<List<TbBets>> GetActiveBets(IUser user)
        {
            var allBets = await _da.GetBetsFromUserId(user.Id);
            var activePollIds = await _da.GetActivePolls().Pipe(x => x.Select(y => y.ID).ToList());

            return allBets.Where(x => activePollIds.Contains(x.PollID)).ToList();
        }
    }
}
