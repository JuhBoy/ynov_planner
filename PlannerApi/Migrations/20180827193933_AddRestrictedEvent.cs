using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddRestrictedEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "restricted_event",
                table: "event",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "event_role",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    event_id = table.Column<int>(nullable: false),
                    role_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_role", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_role_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_role_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_role_event_id",
                table: "event_role",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_role_role_id",
                table: "event_role",
                column: "role_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_role");

            migrationBuilder.DropColumn(
                name: "restricted_event",
                table: "event");
        }
    }
}
