using MarvBotV3.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace MarvBotV3
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TbUsers> tbUsers { get; set; }
        public DbSet<TbCurrency> tbCurrencies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string dbLocation = @"/share/Database/MarvBot.sqlite"; // \\RASPBERRYPI\share\Database\MarvBot.sqlite WINDOWS adress
            options.UseSqlite($"Data Source={dbLocation}");
        }
    }
}
