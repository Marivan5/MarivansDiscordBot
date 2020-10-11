using Discord;
using Discord.WebSocket;
using MarvBotV3.Database;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3
{
    public static class BusinessLayer
    {
        public static async Task SaveGold(IUser user, SocketGuild guild, int amount)
        {
            await DataAccess.SaveGold(user, guild.Id, amount);
            await CheckRichestPerson(guild);
        }

        private static async Task CheckRichestPerson(SocketGuild guild)
        {
            var newRichestPerson = DataAccess.GetTopXGold(1).FirstOrDefault()?.UserID ?? 0;

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
        
        private static SocketGuildUser GetCurrentRichestPerson(SocketGuild guild)
        {
            return guild.Users.FirstOrDefault(x => x.Roles.Any(y => y.Id == ServerConfig.Load().richRole));
        }
    }
}
