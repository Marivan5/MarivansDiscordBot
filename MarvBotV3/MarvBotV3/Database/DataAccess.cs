using Discord;
using Discord.WebSocket;
using MarvBotV3.Database.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Database
{
    public static class DataAccess
    {
        public static int GetGold(ulong userID)
        {
            using (var db = new DatabaseContext())
            {
                if(!db.tbCurrencies.Any(x => x.UserID == userID))
                {
                    return 0;
                }
                var value = db.tbCurrencies.Where(x => x.UserID == userID).Select(x => x.GoldAmount).FirstOrDefault();
                return Convert.ToInt32(value);
            }
        }

        public static async Task SaveGold(IUser user, int amount)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.tbCurrencies.Any(x => x.UserID == user.Id))
                {
                    db.tbCurrencies.Add(new TbCurrency
                    {
                        UserID = user.Id,
                        Username = user.Username,
                        GoldAmount = amount,
                    });
                }
                else
                {
                    TbCurrency tbUser = db.tbCurrencies.Where(x => x.UserID == user.Id).FirstOrDefault();
                    tbUser.GoldAmount += amount;
                    tbUser.Username = user.Username;
                    db.tbCurrencies.Update(tbUser);
                }
                await db.SaveChangesAsync();
            }
        }

        public static async Task GiveGoldEveryone(List<SocketGuildUser> users, int amount)
        {
            using (var db = new DatabaseContext())
            {
                foreach (var user in users)
                {
                    if (!db.tbCurrencies.Any(x => x.UserID == user.Id))
                    {
                        db.tbCurrencies.Add(new TbCurrency
                        {
                            UserID = user.Id,
                            Username = user.Username,
                            GoldAmount = amount,
                        });
                    }
                    else
                    {
                        TbCurrency tbUser = db.tbCurrencies.Where(x => x.UserID == user.Id).FirstOrDefault();
                        tbUser.GoldAmount += amount;
                        tbUser.Username = user.Username;
                        db.tbCurrencies.Update(tbUser);
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
