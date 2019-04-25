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
            // pi: /share/Database/MarvBot.sqlite
            // windows: \\RASPBERRYPI\share\Database\MarvBot.sqlite
            string dbLocation = @"/share/Database/MarvBot.sqlite"; 
            options.UseSqlite($"Data Source={dbLocation}");
        }
    }
}
