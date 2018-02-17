﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddDefaultValueForPromotions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "promotion",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "promotion",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "promotion",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "promotion",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}