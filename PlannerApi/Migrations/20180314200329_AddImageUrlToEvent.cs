using Microsoft.EntityFrameworkCore.Migrations;

namespace events_planner.Migrations
{
    public partial class AddImageUrlToEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "event",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                table: "event");
        }
    }
}
