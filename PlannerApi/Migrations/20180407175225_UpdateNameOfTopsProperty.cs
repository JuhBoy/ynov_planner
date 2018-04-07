using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class UpdateNameOfTopsProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Indexe",
                table: "top_events",
                newName: "Index");

            migrationBuilder.RenameIndex(
                name: "IX_top_events_Indexe",
                table: "top_events",
                newName: "IX_top_events_Index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Index",
                table: "top_events",
                newName: "Indexe");

            migrationBuilder.RenameIndex(
                name: "IX_top_events_Index",
                table: "top_events",
                newName: "IX_top_events_Indexe");
        }
    }
}
