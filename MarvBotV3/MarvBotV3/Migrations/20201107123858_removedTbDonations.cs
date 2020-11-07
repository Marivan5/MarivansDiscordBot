using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class removedTbDonations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbDonations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbDonations",
                columns: table => new
                {
                    ID = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DonationAmount = table.Column<int>(type: "int", nullable: false),
                    GuildID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserID = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Username = table.Column<string>(type: "varchar(64) CHARACTER SET utf8mb4", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbDonations", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TbDonations_TimeStamp",
                table: "TbDonations",
                column: "TimeStamp");
        }
    }
}
