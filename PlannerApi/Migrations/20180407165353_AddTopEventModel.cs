using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace events_planner.Migrations
{
    public partial class AddTopEventModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "top_events_id",
                table: "event",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "top_events",
                columns: table => new
                {
                    top_events_id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    event_id = table.Column<int>(nullable: false),
                    Indexe = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_top_events", x => x.top_events_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_top_events_id",
                table: "event",
                column: "top_events_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_top_events_Indexe",
                table: "top_events",
                column: "Indexe",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_event_top_events_top_events_id",
                table: "event",
                column: "top_events_id",
                principalTable: "top_events",
                principalColumn: "top_events_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_top_events_top_events_id",
                table: "event");

            migrationBuilder.DropTable(
                name: "top_events");

            migrationBuilder.DropIndex(
                name: "IX_event_top_events_id",
                table: "event");

            migrationBuilder.DropColumn(
                name: "top_events_id",
                table: "event");
        }
    }
}
