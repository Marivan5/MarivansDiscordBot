using Microsoft.EntityFrameworkCore.Migrations;

namespace MarvBotV3.Migrations
{
    public partial class TbUserActivitiesUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activity",
                table: "TbUserActivities");

            migrationBuilder.DropColumn(
                name: "Started",
                table: "TbUserActivities");

            migrationBuilder.AddColumn<string>(
                name: "AfterActivity",
                table: "TbUserActivities",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeforeActivity",
                table: "TbUserActivities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterActivity",
                table: "TbUserActivities");

            migrationBuilder.DropColumn(
                name: "BeforeActivity",
                table: "TbUserActivities");

            migrationBuilder.AddColumn<string>(
                name: "Activity",
                table: "TbUserActivities",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Started",
                table: "TbUserActivities",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
