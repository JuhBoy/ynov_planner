using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class ChangeIntToFloatForPriceAndJP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "price",
                table: "price",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<float>(
                name: "points",
                table: "jury_point",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<float>(
                name: "jury_point",
                table: "event",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "price",
                table: "price",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.AlterColumn<int>(
                name: "points",
                table: "jury_point",
                nullable: false,
                oldClrType: typeof(float));

            migrationBuilder.AlterColumn<int>(
                name: "jury_point",
                table: "event",
                nullable: true,
                oldClrType: typeof(float),
                oldNullable: true);
        }
    }
}
