using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class CalendarDays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbCalendarDays",
                columns: table => new
                {
                    CalendarDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    BankDay = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    WeekYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbCalendarDays", x => x.CalendarDate);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbCalendarDays");
        }
    }
}
