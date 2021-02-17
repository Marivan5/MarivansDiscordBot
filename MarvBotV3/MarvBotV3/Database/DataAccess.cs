using Discord;
using Discord.WebSocket;
using MarvBotV3.Database.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Database
{
    public class DataAccess
    {
        DatabaseContext db;

        public DataAccess(DatabaseContext dbContext)
        {
            db = dbContext;
        }

        public int GetGold(ulong userID)
        {
            if (!db.TbCurrencies.Any(x => x.UserID == userID))
                return 0;

            var value = db.TbCurrencies.AsQueryable().Where(x => x.UserID == userID).Select(x => x.GoldAmount).FirstOrDefault();
            return Convert.ToInt32(value);
        }

        public async Task DeleteUser(ulong userID)
        {
            if (!db.TbCurrencies.Any(x => x.UserID == userID))
                return;

            var user = db.TbCurrencies.AsQueryable().Where(x => x.UserID == userID).FirstOrDefault();
            db.TbCurrencies.Remove(user);
            await db.SaveChangesAsync();
        }

        public List<TbCurrency> GetTopXGold(int amount = 10)
        {
            var value = db.TbCurrencies.AsQueryable().Where(x => x.UserID != 276456075559960576).OrderByDescending(x => x.GoldAmount).Take(amount).ToList();
            return value;
        }

        public async Task SaveGold(IUser user, ulong guildID, int amount)
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

        public int GetGambleAmount(IUser user)
        {
            if (!db.TbCurrencies.Any(x => x.UserID == user.Id))
                return 0;

            var value = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).Select(x => x.AmountOfGambles).FirstOrDefault();
            return Convert.ToInt32(value);
        }

        public async Task UpdateGambleAmount(IUser user)
        {
            TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
            tbUser.AmountOfGambles++;
            db.TbCurrencies.Update(tbUser);
            await db.SaveChangesAsync();
        }

        public async Task SaveGoldToBot(int amount)
        {
            TbCurrency tbUser = db.TbCurrencies.AsQueryable().Where(x => x.UserID == 276456075559960576).FirstOrDefault();
            tbUser.GoldAmount += amount;
            db.TbCurrencies.Update(tbUser);
            await db.SaveChangesAsync();
        }

        public async Task GiveGoldEveryone(List<SocketGuildUser> users, int amount)
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

        public async Task SaveStats(IUser user, bool won, long betAmount, long changeAmount, int roll)
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

        public List<TbGoldGambles> GetStats(ulong userID, DateTime? fromDate = null)
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

        public List<TbTempData> GetTempDataAsync()
        {
            var dbValue = db.TbTempData.AsQueryable().OrderByDescending(x => x.Id).Take(100).ToList();

            var value =
                 from x in dbValue
                 group x by x.Room into g
                 select g.OrderByDescending(x => x.Time).First();

            return value.ToList();
        }

        public async Task SetDuel(ulong challenger, ulong challenge, ulong winner, int betAmount)
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

        public List<TbDuels> GetDuelStats(ulong userID)
        {
            if (!db.TbDuels.Any(x => x.Challenger == userID || x.Challenge == userID))
                return null;

            var value = db.TbDuels.AsQueryable().Where(x => x.Challenger == userID || x.Challenge == userID).ToList();
            return value;
        }

        public async Task SaveUserAcitivity(IUser user, string beforeActivity, string afterActivity)
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

        public TbDonations GetLatestDonation(ulong guildId)
        {
            return db.TbDonations.AsQueryable().Where(x => x.GuildID == guildId).OrderByDescending(x => x.TimeStamp).FirstOrDefault();
        }

        public async Task SetDonation(IUser user, ulong guildId, int donationAmount)
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

        public async Task SaveNextRoll(List<int> nextRolls, IUser user = null)
        {
            ulong userId = 0;
            var username = "Anyone";
            if (user != null)
            {
                userId = user.Id;
                username = user.Username;
            }

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

        public TbNextRoll GetNextRoll(ulong userId = 0, bool remove = false)
        {
            var nextRoll = db.TbNextRoll.AsQueryable().FirstOrDefault(x => x.UserID == 0 || x.UserID == userId);

            if (remove && nextRoll != null)
            {
                db.TbNextRoll.Remove(nextRoll);
                db.SaveChanges();
            }

            return nextRoll;
        }

        public async Task SaveBirthday(IUser user, DateTime birthday)
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

        public async Task UpdateBirthday(IUser user, DateTime birthday)
        {
            var tbBirthdays = db.TbBirthdays.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
            tbBirthdays.Birthday = birthday;
            await db.SaveChangesAsync();
        }

        public async Task UpdateBirthdayLastGiftGiven(IUser user, DateTime giftTime)
        {
            var tbBirthdays = db.TbBirthdays.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
            tbBirthdays.LastGiftGiven = giftTime;
            await db.SaveChangesAsync();
        }

        public List<TbBirthdays> GetTodaysBirthdaysWithoutGift()
        {
            return db.TbBirthdays.AsQueryable().Where(x => x.Birthday.Month == DateTime.Today.Month && x.Birthday.Day == DateTime.Today.Day && x.Birthday.Date != x.LastGiftGiven.Date).ToList();
        }
    }
}
