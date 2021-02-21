using Discord;
using Discord.WebSocket;
using MarvBotV3.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3
{
    public class MarvBotBusinessLayer
    {
        private DataAccess _da;

        public MarvBotBusinessLayer(DataAccess da)
        {
            _da = da; 
        }

        public async Task SaveGold(IUser user, SocketGuild guild, int amount)
        {
            await _da.SaveGold(user, guild.Id, amount);
            await CheckRichestPerson(guild);
        }

        private async Task CheckRichestPerson(SocketGuild guild)
        {
            var newRichestPerson = (await _da.GetTopXGold(1)).FirstOrDefault()?.UserID ?? 0;

            if (newRichestPerson == 0)
                return;

            var currentRichestPerson = GetCurrentRichestPerson(guild);
            if(currentRichestPerson == null || newRichestPerson != currentRichestPerson.Id)
            {
                var guildRole = (IRole)guild.GetRole(ServerConfig.Load().richRole);
                await guild.GetUser(newRichestPerson).AddRoleAsync(guildRole);

                if(currentRichestPerson != null)
                    await currentRichestPerson.RemoveRoleAsync(guildRole);
            }
        }
        
        public SocketGuildUser GetCurrentRichestPerson(SocketGuild guild)
        {
            return guild.Users.FirstOrDefault(x => x.Roles.Any(y => y.Id == ServerConfig.Load().richRole));
        }

        public async Task SaveUserAcitivity(IUser user, string beforeActivity, string afterActivity)
        {
            await _da.SaveUserAcitivity(user, beforeActivity, afterActivity);
        }

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
    }
}
