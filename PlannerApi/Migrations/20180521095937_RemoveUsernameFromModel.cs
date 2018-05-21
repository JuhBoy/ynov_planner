using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class RemoveUsernameFromModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_username_email",
                table: "user");

            migrationBuilder.DropColumn(
                name: "username",
                table: "user");

            migrationBuilder.CreateIndex(
                name: "IX_user_email",
                table: "user",
                column: "email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_email",
                table: "user");

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "user",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_username_email",
                table: "user",
                columns: new[] { "username", "email" },
                unique: true);
        }
    }
}
