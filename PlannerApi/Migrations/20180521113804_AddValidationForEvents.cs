using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddValidationForEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "validation_number",
                table: "event",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "validation_required",
                table: "event",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "validated",
                table: "booking",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "validation_number",
                table: "event");

            migrationBuilder.DropColumn(
                name: "validation_required",
                table: "event");

            migrationBuilder.DropColumn(
                name: "validated",
                table: "booking");
        }
    }
}
