﻿// <auto-generated />
using System;
using MarvBotV3;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarvBotV3.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20201105212123_timeStampIndex")]
    partial class timeStampIndex
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

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
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.HasKey("UserID");

                    b.ToTable("TbCurrencies");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbDonations", b =>
                {
                    b.Property<ulong>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("DonationAmount")
                        .HasColumnType("int");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Username")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.HasKey("UserID");

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
                        .HasColumnType("datetime(6)");

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
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.Property<bool>("Won")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("ID");

                    b.ToTable("TbGoldGambles");
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
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<float>("Temperature")
                        .HasColumnType("float");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime(6)");

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
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4")
                        .HasMaxLength(200);

                    b.Property<string>("BeforeActivity")
                        .HasColumnType("varchar(200) CHARACTER SET utf8mb4")
                        .HasMaxLength(200);

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasColumnType("varchar(64) CHARACTER SET utf8mb4")
                        .HasMaxLength(64);

                    b.HasKey("ID");

                    b.ToTable("TbUserActivities");
                });
#pragma warning restore 612, 618
        }
    }
}
