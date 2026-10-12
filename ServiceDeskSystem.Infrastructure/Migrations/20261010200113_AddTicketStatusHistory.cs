using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewStatus = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistory_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistory_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistory_ChangedByUserId_ChangedAt",
                table: "TicketStatusHistory",
                columns: new[] { "ChangedByUserId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistory_TicketId",
                table: "TicketStatusHistory",
                column: "TicketId");

            // One-time backfill from the audit log ("Status changed from X to Y", enum names) so
            // existing history is kept. Enum names are mapped to the stored strings and rows for
            // deleted tickets are skipped. ChangedByUserId stays NULL: before this change the audit
            // log recorded the assigned developer instead of the acting user, so it is unreliable.
            // The join compares as strings so non-numeric EntityId values cannot raise conversion
            // errors in strict mode.
            migrationBuilder.Sql("""
                INSERT INTO `TicketStatusHistory` (`TicketId`, `OldStatus`, `NewStatus`, `ChangedByUserId`, `ChangedAt`)
                SELECT x.`TicketId`, x.`OldStatus`, x.`NewStatus`, NULL, x.`ChangedAt`
                FROM (
                    SELECT
                        t.`Id` AS `TicketId`,
                        REPLACE(REPLACE(SUBSTRING_INDEX(SUBSTRING_INDEX(a.`Changes`, ' from ', -1), ' to ', 1),
                            'InProgress', 'In Progress'), 'CodeReview', 'Code Review') AS `OldStatus`,
                        REPLACE(REPLACE(SUBSTRING_INDEX(a.`Changes`, ' to ', -1),
                            'InProgress', 'In Progress'), 'CodeReview', 'Code Review') AS `NewStatus`,
                        a.`Timestamp` AS `ChangedAt`,
                        a.`Id` AS `AuditId`
                    FROM `AuditLogs` a
                    JOIN `Tickets` t ON CAST(t.`Id` AS CHAR) = a.`EntityId`
                    WHERE a.`Action` = 'STATUS_UPDATE'
                      AND a.`EntityName` = 'Ticket'
                      AND a.`Changes` LIKE 'Status changed from % to %'
                ) x
                WHERE x.`OldStatus` IN ('New', 'Open', 'In Progress', 'Testing', 'Code Review', 'Resolved', 'Closed', 'Done')
                  AND x.`NewStatus` IN ('New', 'Open', 'In Progress', 'Testing', 'Code Review', 'Resolved', 'Closed', 'Done')
                ORDER BY x.`AuditId`;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketStatusHistory");
        }
    }
}
