using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class newSqlMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbCurrencies",
                columns: table => new
                {
                    UserID = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(nullable: true),
                    GoldAmount = table.Column<long>(nullable: false),
                    GuildID = table.Column<ulong>(nullable: false),
                    AmountOfGambles = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbCurrencies", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "TbDuels",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Challenger = table.Column<ulong>(nullable: false),
                    Challenge = table.Column<ulong>(nullable: false),
                    Winner = table.Column<ulong>(nullable: false),
                    BetAmount = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbDuels", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TbGoldGambles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<ulong>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    Won = table.Column<bool>(nullable: false),
                    Amount = table.Column<long>(nullable: false),
                    Roll = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbGoldGambles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TbTempData",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Room = table.Column<string>(nullable: true),
                    Temperature = table.Column<float>(nullable: false),
                    Humidity = table.Column<float>(nullable: false),
                    Altitude = table.Column<float>(nullable: false),
                    AirPressure = table.Column<float>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbTempData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TbUsers",
                columns: table => new
                {
                    UserID = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbUsers", x => x.UserID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbCurrencies");

            migrationBuilder.DropTable(
                name: "TbDuels");

            migrationBuilder.DropTable(
                name: "TbGoldGambles");

            migrationBuilder.DropTable(
                name: "TbTempData");

            migrationBuilder.DropTable(
                name: "TbUsers");
        }
    }
}
