using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class RenameTopEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "top_events",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Index",
                table: "top_events",
                newName: "index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "top_events",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "index",
                table: "top_events",
                newName: "Index");
        }
    }
}
