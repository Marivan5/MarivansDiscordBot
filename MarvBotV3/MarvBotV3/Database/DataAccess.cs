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
                            GuildID = user.Guild.Id
                        });
                    }
                    else
                    {
                        TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
                        tbUser.GoldAmount += amount;
                        tbUser.Username = user.Username;
                        db.TbCurrencies.Update(tbUser);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public static async Task SaveStats(IUser user, bool won, long betAmount, long changeAmount , int roll)
        {
            using (var db = new DatabaseContext())
            {
                db.TbGoldGambles.Add(new TbGoldGambles
                {
                    UserID = user.Id,
                    Username = user.Username,
                    Won = won,
                    BetAmount = betAmount,
                    ChangeAmount = changeAmount,
                    Roll = roll,
                    TimeStamp = DateTime.Now,
                });
                await db.SaveChangesAsync();
            }
        }

        public static List<TbGoldGambles> GetStats(ulong userID, DateTime? fromDate = null)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.TbGoldGambles.Any(x => x.UserID == userID))
                    return null;

                List<TbGoldGambles> value;
                if (fromDate == null)
                    value = db.TbGoldGambles.AsQueryable().Where(x => x.UserID == userID).ToList();
                else
                    value = db.TbGoldGambles.AsQueryable().Where(x => x.UserID == userID && x.TimeStamp >= fromDate).ToList();

                return value;
            }
        }

        public static List<TbTempData> GetTempDataAsync()
        {
            using (var db = new DatabaseContext())
            {
                var dbValue = db.TbTempData.AsQueryable().OrderByDescending(x => x.Id).Take(100).ToList();

                var value =
                     from x in dbValue
                     group x by x.Room into g
                     select g.OrderByDescending(x => x.Time).First();

                return value.ToList();
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

        public static async Task SaveUserAcitivity(IUser user, string beforeActivity, string afterActivity)
        {
            using (var db = new DatabaseContext())
            {
                db.TbUserActivities.Add(new TbUserActivity
                {
                    UserID = user.Id,
                    Username = user.Username,
                    BeforeActivity = beforeActivity,
                    AfterActivity = afterActivity,
                    TimeStamp = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
        }

        public static TbDonations GetLatestDonation(ulong guildId)
        {
            using (var db = new DatabaseContext())
            {
                return db.TbDonations.AsQueryable().Where(x => x.GuildID == guildId).OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            }
        }

        public static async Task SetDonation(IUser user, ulong guildId, int donationAmount)
        {
            using (var db = new DatabaseContext())
            {
                db.TbDonations.Add(new TbDonations
                {
                    UserID = user.Id,
                    Username = user.Username,
                    GuildID = guildId,
                    DonationAmount = donationAmount,
                    TimeStamp = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
        }

        public static async Task SaveNextRoll(List<int> nextRolls, IUser user = null)
        {
            ulong userId = 0;
            var username = "Anyone";
            if(user != null)
            {
                userId = user.Id;
                username = user.Username;
            }

            using (var db = new DatabaseContext())
            {
                foreach (var roll in nextRolls)
                {
                    db.TbNextRoll.Add(new TbNextRoll
                    {
                        UserID = userId,
                        Username = username,
                        NextRoll = roll,
                        TimeStamp = DateTime.Now
                    });
                }
                await db.SaveChangesAsync();
            }
        }

        public static TbNextRoll GetNextRoll(ulong userId = 0, bool remove = false)
        {
            using (var db = new DatabaseContext())
            {
                var nextRoll =  db.TbNextRoll.AsQueryable().FirstOrDefault(x => x.UserID == 0 || x.UserID == userId);

                if (remove && nextRoll != null)
                {
                    db.TbNextRoll.Remove(nextRoll);
                    db.SaveChanges();
                }

                return nextRoll;
            }
        }

        public static async Task SaveBirthday(IUser user, DateTime birthday)
        {
            using (var db = new DatabaseContext())
            {
                db.TbBirthdays.Add(new TbBirthdays
                {
                    UserID = user.Id,
                    Username = user.Username,
                    Birthday = birthday,
                    LastGiftGiven = DateTime.MinValue
                });
                await db.SaveChangesAsync();
            }
        }
        
        public static async Task UpdateBirthday(IUser user, DateTime birthday)
        {
            using (var db = new DatabaseContext())
            {
                TbBirthdays tbBirthdays = db.TbBirthdays.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
                tbBirthdays.Birthday = birthday;
                await db.SaveChangesAsync();
            }
        }
        
        public static async Task UpdateBirthdayLastGiftGiven(IUser user, DateTime giftTime)
        {
            using (var db = new DatabaseContext())
            {
                TbBirthdays tbBirthdays = db.TbBirthdays.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
                tbBirthdays.LastGiftGiven = giftTime;
                await db.SaveChangesAsync();
            }
        }

        public static List<TbBirthdays> GetTodaysBirthdaysWithoutGift()
        {
            using (var db = new DatabaseContext())
            {
                return db.TbBirthdays.AsQueryable().Where(x => x.Birthday.Month == DateTime.Today.Month && x.Birthday.Day == DateTime.Today.Day && x.Birthday.Date != x.LastGiftGiven.Date).ToList();
            }
        }
    }
}
