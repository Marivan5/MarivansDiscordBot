using Discord;
using Discord.WebSocket;
using MarvBotV3.Database.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarvBotV3.Database
{
    public class DataAccess
    {
        readonly DatabaseContext db;

        public DataAccess(DatabaseContext dbContext)
        {
            db = dbContext;
        }

        public Task SaveChanges() => 
            db.SaveChangesAsync();

        public async Task<int> GetGold(ulong userID)
        {
            if (!db.TbCurrencies.Any(x => x.UserID == userID))
                return 0;

            var value = await db.TbCurrencies.AsQueryable().Where(x => x.UserID == userID).Select(x => x.GoldAmount).SingleOrDefaultAsync();
            return Convert.ToInt32(value);
        }

        public async Task DeleteUser(ulong userID)
        {
            if (!db.TbCurrencies.Any(x => x.UserID == userID))
                return;

            var user = await db.TbCurrencies.AsQueryable().Where(x => x.UserID == userID).FirstOrDefaultAsync();
            db.TbCurrencies.Remove(user);
            await db.SaveChangesAsync();
        }

        public Task<List<TbCurrency>> GetTopXGold(int amount = 10) =>
            db.TbCurrencies.AsQueryable()
            .Where(x => x.UserID != 276456075559960576)
            .OrderByDescending(x => x.GoldAmount)
            .Take(amount).ToListAsync();

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

        public async Task<int> GetGambleAmount(IUser user)
        {
            if (!db.TbCurrencies.Any(x => x.UserID == user.Id))
                return 0;

            var value = await db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).Select(x => x.AmountOfGambles).SingleOrDefaultAsync();
            return Convert.ToInt32(value);
        }

        public async Task UpdateGambleAmount(IUser user)
        {
            TbCurrency tbUser = await db.TbCurrencies.AsQueryable().Where(x => x.UserID == user.Id).SingleOrDefaultAsync();
            tbUser.AmountOfGambles++;
            db.TbCurrencies.Update(tbUser);
            await db.SaveChangesAsync();
        }

        public async Task SaveGoldToBot(int amount)
        {
            TbCurrency tbUser = await db.TbCurrencies.AsQueryable().Where(x => x.UserID == 276456075559960576).SingleOrDefaultAsync();
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
                    TbCurrency tbUser = await db.TbCurrencies.AsQueryable().SingleOrDefaultAsync(x => x.UserID == user.Id);
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

        public Task<List<TbGoldGambles>> GetStats(ulong userID, DateTime? fromDate = null)
        {
            if (!db.TbGoldGambles.Any(x => x.UserID == userID))
                return null;

            if (fromDate == null)
                return db.TbGoldGambles
                    .AsQueryable()
                    .Where(x => x.UserID == userID)
                    .ToListAsync();
            else
                return db.TbGoldGambles
                    .AsQueryable()
                    .Where(x => x.UserID == userID && x.TimeStamp >= fromDate)
                    .ToListAsync();
        }

        public async Task<List<TbTempData>> GetTempDataAsync()
        {
            var dbValue = await db.TbTempData
                .AsQueryable()
                .OrderByDescending(x => x.Id)
                .Take(100)
                .ToListAsync();

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

        public async Task<List<TbDuels>> GetDuelStats(ulong userID)
        {
            var exists = await db.TbDuels.AsQueryable().AnyAsync(x => x.Challenger == userID || x.Challenge == userID);

            if (!exists)
                return null;

            return await db.TbDuels
                .AsQueryable()
                .Where(x => x.Challenger == userID 
                    || x.Challenge == userID)
                .ToListAsync();
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

        public Task<TbDonations> GetLatestDonation(ulong guildId) =>
            db.TbDonations.AsQueryable().Where(x => x.GuildID == guildId).OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();

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

        public async Task<TbNextRoll> GetNextRoll(ulong userId = 0, bool remove = false)
        {
            var nextRoll = await db.TbNextRoll.AsQueryable().FirstOrDefaultAsync(x => x.UserID == 0 || x.UserID == userId);

            if (remove && nextRoll != null)
            {
                db.TbNextRoll.Remove(nextRoll);
                await db.SaveChangesAsync();
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

        public Task<TbBirthdays> GetBirthday(IUser user) =>
            db.TbBirthdays.AsQueryable().SingleOrDefaultAsync(x => x.UserID == user.Id);

        public Task<List<TbBirthdays>> GetBirthdays() => 
            db.TbBirthdays.AsQueryable().ToListAsync();

        public async Task UpdateBirthday(IUser user, DateTime birthday)
        {
            var tbBirthdays = db.TbBirthdays.AsQueryable().Where(x => x.UserID == user.Id).FirstOrDefault();
            tbBirthdays.Birthday = birthday;
            await db.SaveChangesAsync();
        }

        public async Task UpdateBirthdayLastGiftGiven(IUser user, DateTime giftTime)
        {
            var tbBirthdays = await db.TbBirthdays
                .AsQueryable()
                .Where(x => x.UserID == user.Id)
                .SingleOrDefaultAsync();
            tbBirthdays.LastGiftGiven = giftTime;
            await db.SaveChangesAsync();
        }

        public Task<List<TbBirthdays>> GetTodaysBirthdaysWithoutGift() =>
            db.TbBirthdays
                .AsQueryable()
                .Where(x => x.Birthday.Month == DateTime.Today.Month
                    && x.Birthday.Day == DateTime.Today.Day
                    && DateTime.Now.Date != x.LastGiftGiven.Date)
                .ToListAsync();

        public async Task SetCalendarDays(List<TbCalendarDays> days)
        {
            db.TbCalendarDays.AddRange(days);
            await db.SaveChangesAsync();
        }

        public Task<List<TbCalendarDays>> GetCalendarDaysForYear(int year) => 
            db.TbCalendarDays.AsQueryable().Where(x => x.CalendarDate.Year == year).ToListAsync();

        public Task<List<TbPolls>> GetActivePolls() => 
            db.TbPolls.AsQueryable().Where(x => x.Result == null).ToListAsync();

        public async Task<long> SaveNewPoll(string name, ulong creator)
        {
            var tbPoll = new TbPolls
            {
                CreatorUserID = creator,
                Name = name,
                CreatedTimeStamp = DateTime.Now,
            };
            db.TbPolls.Add(tbPoll);
            await db.SaveChangesAsync();
            return tbPoll.ID;
        }
        
        public async Task SetResultPoll(long id, bool result)
        {
            var tbPoll = await db.TbPolls
                .AsQueryable()
                .Where(x => x.ID == id && x.Result == null)
                .SingleOrDefaultAsync();

            tbPoll.Result = result;
            tbPoll.ResultTimeStamp = DateTime.Now;
            await db.SaveChangesAsync();
        }

        public Task<List<TbBets>> GetBetsFromPollId(long id) => 
            db.TbBets.AsQueryable().Where(x => x.PollID == id).ToListAsync();

        public Task<List<TbBets>> GetBetsFromUserId(ulong id) =>
            db.TbBets.AsQueryable().Where(x => x.UserID == id).ToListAsync();

        public async Task SaveNewBet(int id, bool result, int amount, IUser user)
        {
            var activePoll = await GetActivePolls().Pipe(x => x.Where(y => y.ID == id).ToList());

            if (!activePoll.Any())
                return;

            // Is there active poll with this iD?
            db.TbBets.Add(new TbBets
            {
                PollID = id,
                UserID = user.Id,
                Bet = result,
                BetAmount = amount,
                TimeStamp = DateTime.Now
            });
            await db.SaveChangesAsync();
        }
    }
}
