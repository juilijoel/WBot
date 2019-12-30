using Microsoft.EntityFrameworkCore.Migrations;

namespace WBot.DataAccess.Migrations
{
    public partial class FirstPostMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FirstPostMessageId",
                table: "Links",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstPostMessageId",
                table: "Links");
        }
    }
}
