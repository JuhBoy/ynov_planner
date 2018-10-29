using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace events_planner.Migrations
{
    public partial class ChangeBookingToBeNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "present",
                table: "booking",
                nullable: true,
                oldClrType: typeof(bool));

            //======================================================================== 
            //     Should assert that all booking as been nullyfied pre-event
            //========================================================================
            migrationBuilder.Sql(@"
                UPDATE
                    booking
                SET
                    booking.present = NULL
                WHERE
                    booking.event_id IN (SELECT event_id FROM event WHERE close_at > NOW())
                AND
                    booking.present = FALSE
            ");

            //======================================================================== 
            //     Create all the booking malus for non presence on events
            //========================================================================
            migrationBuilder.Sql(@"
                INSERT INTO
                    jury_point (points, user_id, event_id, description)
                    (
                        SELECT
                            ROUND(-ev.jury_point * 3, 2) as nPoints,
                            booking.user_id,
                            booking.event_id,
                            CONCAT('Event:Abs:', booking.event_id) as descr
                        FROM
                            booking
                        JOIN
                            event as ev
                        ON
                            ev.event_id = booking.event_id
                        WHERE
                                ev.close_at < NOW()
                            AND
                                ev.jury_point IS NOT NULL
                            AND
                                booking.present = FALSE
                    )
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dont force back to false value: Null value are settled FALSE by default on Bit(1) type
            migrationBuilder.AlterColumn<bool>(
                name: "present",
                table: "booking",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            // Simply remove all the s
            migrationBuilder.Sql(@"DELETE FROM jury_point WHERE points <= 0");
        }
    }
}
