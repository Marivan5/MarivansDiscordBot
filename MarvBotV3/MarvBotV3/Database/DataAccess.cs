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

        public static async Task DeleteUser(ulong userID)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.tbCurrencies.Any(x => x.UserID == userID))
                {
                    return;
                }
                var user = db.tbCurrencies.Where(x => x.UserID == userID).FirstOrDefault();
                db.tbCurrencies.Remove(user);
                await db.SaveChangesAsync();
            }
        }

        public static List<TbCurrency> GetAllGold(ulong guildID, int amount = 10)
        {
            using (var db = new DatabaseContext())
            {
                var value = db.tbCurrencies.OrderByDescending(x => x.GoldAmount).Take(amount).ToList();
                return value;
            }
        }

        public static async Task SaveGold(IUser user, ulong guildID, int amount)
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
                        GuildID = guildID,
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

        public static int GetGambleAmount(IUser user)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.tbCurrencies.Any(x => x.UserID == user.Id))
                {
                    return 0;
                }
                var value = db.tbCurrencies.Where(x => x.UserID == user.Id).Select(x => x.AmountOfGambles).FirstOrDefault();
                return Convert.ToInt32(value);
            }
        }

        public static async Task UpdateGambleAmount(IUser user)
        {
            using (var db = new DatabaseContext())
            {
                TbCurrency tbUser = db.tbCurrencies.Where(x => x.UserID == user.Id).FirstOrDefault();
                tbUser.AmountOfGambles++;
                db.tbCurrencies.Update(tbUser);
                await db.SaveChangesAsync();
            }
        }

        public static async Task SaveGoldToBot(int amount)
        {
            using (var db = new DatabaseContext())
            {
                TbCurrency tbUser = db.tbCurrencies.Where(x => x.UserID == 276456075559960576).FirstOrDefault();
                tbUser.GoldAmount += amount;
                db.tbCurrencies.Update(tbUser);
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
                            AmountOfGambles = 0,
                        });
                    }
                    else
                    {
                        TbCurrency tbUser = db.tbCurrencies.Where(x => x.UserID == user.Id).FirstOrDefault();
                        tbUser.GoldAmount += amount;
                        tbUser.Username = user.Username;
                        tbUser.AmountOfGambles = 0;
                        db.tbCurrencies.Update(tbUser);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public static async Task SaveStats(IUser user, bool won, int amount, int roll)
        {
            using (var db = new DatabaseContext())
            {
                db.tbGoldGambles.Add(new tbGoldGambles
                {
                    UserID = user.Id,
                    Username = user.Username,
                    Won = won,
                    Amount = (long)amount,
                    Roll = roll,
                    TimeStamp = DateTime.Now,
                });
                await db.SaveChangesAsync();
            }
        }

        public static List<tbGoldGambles> GetStats(ulong userID)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.tbGoldGambles.Any(x => x.UserID == userID))
                {
                    return null;
                }
                var value = db.tbGoldGambles.Where(x => x.UserID == userID).ToList();
                return value;
            }
        }
    }
}
