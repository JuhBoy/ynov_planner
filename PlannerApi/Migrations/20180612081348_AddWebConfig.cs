using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddWebConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "web_config",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    facebook_url = table.Column<string>(nullable: true),
                    favicon_url = table.Column<string>(nullable: true),
                    instagram_url = table.Column<string>(nullable: true),
                    legal_notice = table.Column<string>(nullable: false),
                    logo_url = table.Column<string>(nullable: true),
                    name = table.Column<string>(nullable: false),
                    session_count = table.Column<int>(nullable: false),
                    subtitle = table.Column<string>(nullable: true),
                    terms_of_usage = table.Column<string>(nullable: false),
                    twitter_url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_config", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "web_config");
        }
    }
}
