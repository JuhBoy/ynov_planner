using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class ChangeNameOfForeignForJuryPoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jury_point_user_UserId",
                table: "jury_point");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "jury_point",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_jury_point_UserId",
                table: "jury_point",
                newName: "IX_jury_point_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_jury_point_user_user_id",
                table: "jury_point",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jury_point_user_user_id",
                table: "jury_point");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "jury_point",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_jury_point_user_id",
                table: "jury_point",
                newName: "IX_jury_point_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_jury_point_user_UserId",
                table: "jury_point",
                column: "UserId",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
