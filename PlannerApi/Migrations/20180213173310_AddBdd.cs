using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class AddBdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    created_at = table.Column<DateTime>(nullable: false),
                    name = table.Column<string>(maxLength: 20, nullable: true),
                    sub_category_id = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_category_category_sub_category_id",
                        column: x => x.sub_category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    event_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    created_at = table.Column<DateTime>(nullable: false),
                    description = table.Column<string>(nullable: false),
                    end_at = table.Column<DateTime>(nullable: true),
                    location = table.Column<string>(type: "BLOB", nullable: true),
                    open_at = table.Column<DateTime>(nullable: true),
                    start_at = table.Column<DateTime>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    subscribe_number = table.Column<int>(nullable: false),
                    title = table.Column<string>(maxLength: 255, nullable: true),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "promotion",
                columns: table => new
                {
                    promotion_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    created_at = table.Column<DateTime>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    name = table.Column<string>(maxLength: 40, nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    year = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion", x => x.promotion_id);
                });

            migrationBuilder.CreateTable(
                name: "recovery",
                columns: table => new
                {
                    recovery_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    created_at = table.Column<DateTime>(nullable: false),
                    token = table.Column<string>(maxLength: 200, nullable: true),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery", x => x.recovery_id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    role_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    name = table.Column<string>(maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "eventcategory",
                columns: table => new
                {
                    category_id = table.Column<int>(nullable: false),
                    event_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventcategory", x => new { x.category_id, x.event_id });
                    table.ForeignKey(
                        name: "FK_eventcategory_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventcategory_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventpromotion",
                columns: table => new
                {
                    promotion_id = table.Column<int>(nullable: false),
                    event_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventpromotion", x => new { x.promotion_id, x.event_id });
                    table.ForeignKey(
                        name: "FK_eventpromotion_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventpromotion_promotion_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotion",
                        principalColumn: "promotion_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price",
                columns: table => new
                {
                    price_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    price = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    event_id = table.Column<int>(nullable: false),
                    role_id = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price", x => x.price_id);
                    table.ForeignKey(
                        name: "FK_price_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_price_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    created_at = table.Column<DateTime>(nullable: false),
                    date_of_birth = table.Column<DateTime>(nullable: true),
                    email = table.Column<string>(maxLength: 30, nullable: false),
                    first_name = table.Column<string>(maxLength: 20, nullable: false),
                    last_name = table.Column<string>(maxLength: 20, nullable: false),
                    password = table.Column<string>(nullable: false),
                    phone_number = table.Column<int>(nullable: false),
                    promotion_id = table.Column<int>(nullable: false),
                    role_id = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    username = table.Column<string>(maxLength: 20, nullable: true),
                    recovery_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_promotion_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotion",
                        principalColumn: "promotion_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_recovery_recovery_id",
                        column: x => x.recovery_id,
                        principalTable: "recovery",
                        principalColumn: "recovery_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    created_at = table.Column<DateTime>(nullable: false),
                    event_id = table.Column<int>(nullable: false),
                    present = table.Column<bool>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_booking_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventuser",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    event_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventuser", x => new { x.user_id, x.event_id });
                    table.ForeignKey(
                        name: "FK_eventuser_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventuser_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscribe",
                columns: table => new
                {
                    subcribe_id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    category_id = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false),
                    user_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscribe", x => x.subcribe_id);
                    table.ForeignKey(
                        name: "FK_subscribe_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscribe_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_event_id",
                table: "booking",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_user_id",
                table: "booking",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_sub_category_id",
                table: "category",
                column: "sub_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_eventcategory_event_id",
                table: "eventcategory",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_eventpromotion_event_id",
                table: "eventpromotion",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_eventuser_event_id",
                table: "eventuser",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_price_event_id",
                table: "price",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_price_role_id",
                table: "price",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscribe_category_id",
                table: "subscribe",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscribe_user_id",
                table: "subscribe",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_promotion_id",
                table: "user",
                column: "promotion_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_id",
                table: "user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_recovery_id",
                table: "user",
                column: "recovery_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking");

            migrationBuilder.DropTable(
                name: "eventcategory");

            migrationBuilder.DropTable(
                name: "eventpromotion");

            migrationBuilder.DropTable(
                name: "eventuser");

            migrationBuilder.DropTable(
                name: "price");

            migrationBuilder.DropTable(
                name: "subscribe");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "promotion");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "recovery");
        }
    }
}
