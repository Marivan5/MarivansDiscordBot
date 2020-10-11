using Discord;
using Discord.WebSocket;
using MarvBotV3.Database.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
                if (!db.TbCurrencies.Any(x => x.UserID == userID))
                {
                    return 0;
                }
                var value = db.TbCurrencies.AsQueryable().Where(x => x.UserID == userID).Select(x => x.GoldAmount).FirstOrDefault();
                return Convert.ToInt32(value);
            }
        }

        public static async Task DeleteUser(ulong userID)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.TbCurrencies.Any(x => x.UserID == userID))
                {
                    return;
                }
                var user = db.TbCurrencies.AsQueryable().Where(x => x.UserID == userID).FirstOrDefault();
                db.TbCurrencies.Remove(user);
                await db.SaveChangesAsync();
            }
        }

        public static List<TbCurrency> GetTopXGold(int amount = 10)
        {
            using (var db = new DatabaseContext())
            {
                var value = db.TbCurrencies.AsQueryable().Where(x => x.UserID != 276456075559960576).OrderByDescending(x => x.GoldAmount).Take(amount).ToList();
                return value;
            }
        }

        public static async Task SaveGold(IUser user, ulong guildID, int amount)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.TbCurrencies.Any(x => x.UserID == user.Id))
                {
                    db.TbCurrencies.Add(new TbCurrency
                    {
                        UserID = user.Id,
                        Username = user.Username,
                        GoldAmount = amount,
                        GuildID = guildID,
                    });
                }
                else
                {
                    TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
                    tbUser.GoldAmount += amount;
                    tbUser.Username = user.Username;
                    db.TbCurrencies.Update(tbUser);
                }
                await db.SaveChangesAsync();
            }
        }

        public static int GetGambleAmount(IUser user)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.TbCurrencies.Any(x => x.UserID == user.Id))
                {
                    return 0;
                }
                var value = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).Select(x => x.AmountOfGambles).FirstOrDefault();
                return Convert.ToInt32(value);
            }
        }

        public static async Task UpdateGambleAmount(IUser user)
        {
            using (var db = new DatabaseContext())
            {
                TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
                tbUser.AmountOfGambles++;
                db.TbCurrencies.Update(tbUser);
                await db.SaveChangesAsync();
            }
        }

        public static async Task SaveGoldToBot(int amount)
        {
            using (var db = new DatabaseContext())
            {
                TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == 276456075559960576).FirstOrDefault();
                tbUser.GoldAmount += amount;
                db.TbCurrencies.Update(tbUser);
                await db.SaveChangesAsync();
            }
        }

        public static async Task GiveGoldEveryone(List<SocketGuildUser> users, int amount)
        {
            using (var db = new DatabaseContext())
            {
                foreach (var user in users)
                {
                    if (!db.TbCurrencies.Any(x => x.UserID == user.Id))
                    {
                        db.TbCurrencies.Add(new TbCurrency
                        {
                            UserID = user.Id,
                            Username = user.Username,
                            GoldAmount = amount,
                            AmountOfGambles = 0,
                        });
                    }
                    else
                    {
                        TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
                        tbUser.GoldAmount += amount;
                        tbUser.Username = user.Username;
                        tbUser.AmountOfGambles = 0;
                        db.TbCurrencies.Update(tbUser);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public static async Task SaveStats(IUser user, bool won, int amount, int roll)
        {
            using (var db = new DatabaseContext())
            {
                db.TbGoldGambles.Add(new tbGoldGambles
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
                if (!db.TbGoldGambles.Any(x => x.UserID == userID))
                {
                    return null;
                }
                var value = db.TbGoldGambles.AsQueryable().Where(x => x.UserID == userID).ToList();
                return value;
            }
        }

        public static List<TbTempData> GetTempDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var value =
                     (from x in db.TbTempData.AsEnumerable()
                     group x by x.Room into g
                     select g.OrderByDescending(x => x.Time).First()).ToList();

                //var value = db.TbTempData.AsQueryable().GroupBy(x => x.Room).ForEach(x => x.OrderByDescending(z => z.Time).FirstOrDefault());
                return value;
            }
        }

        public static async Task SetDuel(ulong challenger, ulong challenge, ulong winner, int betAmount)
        {
            using (var db = new DatabaseContext())
            {
                db.TbDuels.Add(new TbDuels
                {
                    Challenger = challenger,
                    Challenge = challenge,
                    Winner = winner,
                    BetAmount = betAmount,
                    TimeStamp = DateTime.Now,
                });
                await db.SaveChangesAsync();
            }
        }

        public static List<TbDuels> GetDuelStats(ulong userID)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.TbDuels.Any(x => x.Challenger == userID || x.Challenge == userID))
                {
                    return null;
                }
                var value = db.TbDuels.AsQueryable().Where(x => x.Challenger == userID || x.Challenge == userID).ToList();
                return value;
            }
        }
    }
}
