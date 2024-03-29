﻿using MarvBotV3.Database.Tables;
using Microsoft.EntityFrameworkCore;
using System;

namespace MarvBotV3
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TbCurrency> TbCurrencies { get; set; }
        public DbSet<TbGoldGambles> TbGoldGambles { get; set; }
        public DbSet<TbTempData> TbTempData { get; set; }
        public DbSet<TbDuels> TbDuels { get; set; }
        public DbSet<TbRockPaperScissors> TbRockPaperScissors { get; set; }
        public DbSet<TbUserActivity> TbUserActivities { get; set; }
        public DbSet<TbDonations> TbDonations { get; set; }
        public DbSet<TbNextRoll> TbNextRoll { get; set; }
        public DbSet<TbCommandsLog> TbCommandsLog { get; set; }
        public DbSet<TbPolls> TbPolls { get; set; }
        public DbSet<TbBets> TbBets { get; set; }
        public DbSet<TbBirthdays> TbBirthdays { get; set; }
        public DbSet<TbCalendarDays> TbCalendarDays { get; set; }
        public DbSet<TbInvestment> TbInvestments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TbDonations>()
                .HasIndex(x => x.TimeStamp);

            modelBuilder.Entity<TbTempData>()
                .HasIndex(x => x.Time);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // pi: /share/Database/MarvBot.sqlite
            // windows: \\RASPBERRYPI\share\Database\MarvBot.sqlite
            //string dbLocation = @"\\RASPBERRYPI\share\Database\MarvBot.sqlite";
            //string dbLocation = @"/share/Database/MarvBot.sqlite"; 
            string dbLocation = "192.168.1.236";
            options.UseMySql($"Server={dbLocation}; Port = 3306; DATABASE = MarvBot; UID=MarvBot; Password=MarvBotV3;",
                new MariaDbServerVersion(new Version(5, 5, 68)),
                mySqlOption => mySqlOption.EnableRetryOnFailure(maxRetryCount: 50, maxRetryDelay: TimeSpan.FromSeconds(60), errorNumbersToAdd: null));
        }
    }
}
