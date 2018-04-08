using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class FixCascadeDeletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_top_events_top_events_id",
                table: "event");

            migrationBuilder.DropIndex(
                name: "IX_event_top_events_id",
                table: "event");

            migrationBuilder.CreateIndex(
                name: "IX_top_events_event_id",
                table: "top_events",
                column: "event_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_top_events_event_event_id",
                table: "top_events",
                column: "event_id",
                principalTable: "event",
                principalColumn: "event_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_top_events_event_event_id",
                table: "top_events");

            migrationBuilder.DropIndex(
                name: "IX_top_events_event_id",
                table: "top_events");

            migrationBuilder.CreateIndex(
                name: "IX_event_top_events_id",
                table: "event",
                column: "top_events_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_event_top_events_top_events_id",
                table: "event",
                column: "top_events_id",
                principalTable: "top_events",
                principalColumn: "top_events_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
