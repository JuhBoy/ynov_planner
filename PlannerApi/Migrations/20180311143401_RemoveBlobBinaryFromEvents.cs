using Microsoft.EntityFrameworkCore.Migrations;

namespace events_planner.Migrations
{
    public partial class RemoveBlobBinaryFromEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "location",
                table: "event",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "BLOB",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "location",
                table: "event",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
