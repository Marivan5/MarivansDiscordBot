using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class GuildIdTbCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildID",
                table: "tbCurrencies",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildID",
                table: "tbCurrencies");
        }
    }
}
