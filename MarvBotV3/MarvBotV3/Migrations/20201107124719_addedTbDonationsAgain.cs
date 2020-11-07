using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class addedTbDonationsAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbDonations",
                columns: table => new
                {
                    ID = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<ulong>(nullable: false),
                    Username = table.Column<string>(maxLength: 64, nullable: true),
                    GuildID = table.Column<ulong>(nullable: false),
                    DonationAmount = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbDonations");
        }
    }
}
