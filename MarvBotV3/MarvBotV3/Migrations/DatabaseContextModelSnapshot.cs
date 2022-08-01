﻿// <auto-generated />
using System;
using MarvBotV3;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MarvBotV3.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbBets", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<bool>("Bet")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("BetAmount")
                        .HasColumnType("int");

                    b.Property<long>("PollID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ID");

                    b.ToTable("TbBets");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbBirthdays", b =>
                {
                    b.Property<ulong>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("LastGiftGiven")
                        .HasColumnType("datetime");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("UserID");

                    b.ToTable("TbBirthdays");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbCalendarDays", b =>
                {
                    b.Property<DateTime>("CalendarDate")
                        .HasColumnType("datetime");

                    b.Property<bool>("BankDay")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("WeekNumber")
                        .HasColumnType("int");

                    b.Property<int>("WeekYear")
                        .HasColumnType("int");

                    b.HasKey("CalendarDate");

                    b.ToTable("TbCalendarDays");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbCommandsLog", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Command")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("ID");

                    b.ToTable("TbCommandsLog");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbCurrency", b =>
                {
                    b.Property<ulong>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("AmountOfGambles")
                        .HasColumnType("int");

                    b.Property<long>("GoldAmount")
                        .HasColumnType("bigint");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("UserID");

                    b.ToTable("TbCurrencies");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbDonations", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("DonationAmount")
                        .HasColumnType("int");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("ID");

                    b.HasIndex("TimeStamp");

                    b.ToTable("TbDonations");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbDuels", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BetAmount")
                        .HasColumnType("int");

                    b.Property<ulong>("Challenge")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Challenger")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("Winner")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ID");

                    b.ToTable("TbDuels");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbGoldGambles", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("BetAmount")
                        .HasColumnType("bigint");

                    b.Property<long>("ChangeAmount")
                        .HasColumnType("bigint");

                    b.Property<int>("Roll")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("Won")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("ID");

                    b.ToTable("TbGoldGambles");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbNextRoll", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("AmountOver")
                        .HasColumnType("int");

                    b.Property<int>("NextRoll")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("ID");

                    b.ToTable("TbNextRoll");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbPolls", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedTimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("CreatorUserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<bool?>("Result")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("ResultTimeStamp")
                        .HasColumnType("datetime");

                    b.HasKey("ID");

                    b.ToTable("TbPolls");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbRockPaperScissors", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("BetAmount")
                        .HasColumnType("int");

                    b.Property<ulong>("Challenge")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("ChallengeChoice")
                        .HasColumnType("longtext");

                    b.Property<ulong>("Challenger")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("ChallengerChoice")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("Winner")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("ID");

                    b.ToTable("TbRockPaperScissors");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbTempData", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<float>("AirPressure")
                        .HasColumnType("float");

                    b.Property<float>("Altitude")
                        .HasColumnType("float");

                    b.Property<float>("Humidity")
                        .HasColumnType("float");

                    b.Property<string>("Room")
                        .HasColumnType("longtext");

                    b.Property<float>("Temperature")
                        .HasColumnType("float");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("Time");

                    b.ToTable("TbTempData");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbUserActivity", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AfterActivity")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("BeforeActivity")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("ID");

                    b.ToTable("TbUserActivities");
                });
#pragma warning restore 612, 618
        }
    }
}
