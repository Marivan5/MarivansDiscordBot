﻿// <auto-generated />
using System;
using MarvBotV3;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarvBotV3.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
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
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("UserID");

                    b.ToTable("TbCurrencies");
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

                    b.ToTable("TbTempData");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.TbUsers", b =>
                {
                    b.Property<ulong>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("UserName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("UserID");

                    b.ToTable("TbUsers");
                });

            modelBuilder.Entity("MarvBotV3.Database.Tables.tbGoldGambles", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("Amount")
                        .HasColumnType("bigint");

                    b.Property<int>("Roll")
                        .HasColumnType("int");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Username")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("Won")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("ID");

                    b.ToTable("TbGoldGambles");
                });
#pragma warning restore 612, 618
        }
    }
}
