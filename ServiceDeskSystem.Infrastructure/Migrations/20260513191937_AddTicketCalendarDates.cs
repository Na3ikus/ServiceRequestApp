using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCalendarDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "DueDate", "StartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "h3j3WvawmAO8p/tCrTORpg==:wkE4MgTZ+WhfQonMByxeJWAm/yfoiPATFrE868tsuCI=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "0VYvpnFxaJLIlWN+QxFcCg==:RJWBOY29mwrEW29FEAiurWlPAs6dcuHz+PIMueYLJIg=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "UTiYr8724zVqUmXuTltVOA==:mGShX6dnA+o9J1eUOnI9txaWY1hqCFPLYexk5nQxk5Y=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "W99bQ184h2M1wIcQTZda0A==:7kF0qsDyhKuUTY703Rb04UiXxG0zIjJtEX/LRPTNnOY=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "sd6tRMxrw2lxFi0B7aRyLg==:y0I0/fgxyMUqSQmBaK9jp7yunWDmPiniOKL5vopySLs=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "rrIkyDhA9OIIHr9qpdqzTA==:2wLlUqq/B3+pJdRDurBlo2Zz9ynnzoO8ankbWxOsXiI=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "HV259ISfrT2fNbTY95z1JA==:SdiK7vWbLZYDUmFPiU0dslGCkLqXVbIZwtMLTW51fTc=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "fodF4UwwT4tyvrikme7NUg==:BL1zJA/j6B5PmZlthmVWcZdZyCE9u8za7S3FkevBZPc=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "+cU+fOsO/rplzOOKRanpCg==:mYfqQQsMKhqJtoTJbbh/EGHPhn9DrQlDUghgrKZoCo4=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "Pp2x1z2DClti/9WD+nQRow==:QDTJasXHjF2Xw+6n+3YkUCOnCDSd1Frez+B6E4sHfv0=");
        }
    }
}
