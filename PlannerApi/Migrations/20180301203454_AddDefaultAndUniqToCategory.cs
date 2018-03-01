using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddDefaultAndUniqToCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "category",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "category",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime));

            migrationBuilder.CreateIndex(
                name: "IX_category_name",
                table: "category",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_category_name",
                table: "category");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "category",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "category",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
