using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class timeStampIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TbTempData_Time",
                table: "TbTempData",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_TbDonations_TimeStamp",
                table: "TbDonations",
                column: "TimeStamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TbTempData_Time",
                table: "TbTempData");

            migrationBuilder.DropIndex(
                name: "IX_TbDonations_TimeStamp",
                table: "TbDonations");
        }
    }
}
