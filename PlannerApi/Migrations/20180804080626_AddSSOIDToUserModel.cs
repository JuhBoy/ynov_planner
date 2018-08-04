using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddSSOIDToUserModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sso_id",
                table: "user",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_sso_id",
                table: "user",
                column: "sso_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_sso_id",
                table: "user");

            migrationBuilder.DropColumn(
                name: "sso_id",
                table: "user");
        }
    }
}
