using MarvBotV3.Database.Tables;
using System;
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

        public static async Task SaveGold(ulong userID, int amount)
        {
            using (var db = new DatabaseContext())
            {
                if (!db.tbCurrencies.Any(x => x.UserID == userID))
                {
                    db.tbCurrencies.Add(new TbCurrency
                    {
                        UserID = userID,
                        GoldAmount = amount,
                    });
                }
                else
                {
                    TbCurrency user = db.tbCurrencies.Where(x => x.UserID == userID).FirstOrDefault();
                    user.GoldAmount += amount;
                    db.tbCurrencies.Update(user);
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
