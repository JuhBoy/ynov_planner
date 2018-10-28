using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class JuryPointsLinkedWithEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "jury_point",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "event_id",
                table: "jury_point",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_jury_point_event_id",
                table: "jury_point",
                column: "event_id");

            migrationBuilder.AddForeignKey(
                name: "FK_jury_point_event_event_id",
                table: "jury_point",
                column: "event_id",
                principalTable: "event",
                principalColumn: "event_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE
                    jury_point AS jpp
                    INNER JOIN
                    (
                        SELECT
                            id,
                            points,
                            UserId,
                            ebjb.event_id,
                            ebjb.booking_id
                        FROM
                            jury_point
                        JOIN
                            (
                                SELECT
                                    ej.event_id, booking.booking_id, jury_point, booking.user_id 
                                FROM
                                    booking 
                                JOIN
                                    (SELECT event_id, jury_point from event WHERE jury_point IS NOT NULL) AS ej
                                ON
                                    ej.event_id = booking.event_id
                                WHERE
                                    booking.present
                            ) AS ebjb
                        ON
                            ebjb.jury_point = jury_point.points && ebjb.user_id = jury_point.UserId
                    ) AS R ON jpp.id = R.id
                SET
                    jpp.event_id = R.event_id,
                    jpp.description = 'From Event';
            ", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jury_point_event_event_id",
                table: "jury_point");

            migrationBuilder.DropIndex(
                name: "IX_jury_point_event_id",
                table: "jury_point");

            migrationBuilder.DropColumn(
                name: "description",
                table: "jury_point");

            migrationBuilder.DropColumn(
                name: "event_id",
                table: "jury_point");
        }
    }
}
