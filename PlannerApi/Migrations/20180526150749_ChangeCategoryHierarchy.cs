using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class ChangeCategoryHierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_category_category_sub_category_id",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "sub_category_id",
                table: "category",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_category_sub_category_id",
                table: "category",
                newName: "IX_category_ParentId");

            migrationBuilder.AddColumn<int>(
                name: "parent_category_id",
                table: "category",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_category_category_ParentId",
                table: "category",
                column: "ParentId",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_category_category_ParentId",
                table: "category");

            migrationBuilder.DropColumn(
                name: "parent_category_id",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "category",
                newName: "sub_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_category_ParentId",
                table: "category",
                newName: "IX_category_sub_category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_category_category_sub_category_id",
                table: "category",
                column: "sub_category_id",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
