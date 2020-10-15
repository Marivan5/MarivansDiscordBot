using MarvBotV3.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace MarvBotV3
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TbUsers> TbUsers { get; set; }
        public DbSet<TbCurrency> TbCurrencies { get; set; }
        public DbSet<tbGoldGambles> TbGoldGambles { get; set; }
        public DbSet<TbTempData> TbTempData { get; set; }
        public DbSet<TbDuels> TbDuels { get; set; }
        public DbSet<TbUserActivity> TbUserActivities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // pi: /share/Database/MarvBot.sqlite
            // windows: \\RASPBERRYPI\share\Database\MarvBot.sqlite
            //string dbLocation = @"\\RASPBERRYPI\share\Database\MarvBot.sqlite";
            //string dbLocation = @"/share/Database/MarvBot.sqlite"; 
            string dbLocation = "192.168.1.236"; 
            options.UseMySql($"Server={dbLocation}; Port = 3306; DATABASE = MarvBot; UID=MarvBot; Password=MarvBotV3;");
        }
    }
}
