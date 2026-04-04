using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RecipientUserId = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TicketId",
                table: "Notifications",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "kJOL24z9kdYsvj2p50HqhQ==:bA2nX8+cuPsjalxUErx+bwQIAHWvRAdff6t356Om6CM=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "VaPgxY0WNokXod5c5Xgunw==:FEIdsZCzjWowxLgKT0pZuOTD/Z6lM+hiATOELRUnGwU=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "Nb6ZfgCqLqw4mff67MpV7g==:jnjXnUvh/18mKptEb+5MpxnLEK9CVkWu9y9KkHoVv4g=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "xFbLj/N68PcRjmRRa1YdZw==:Dj136CX/iqZSJHU423o0Y4pRMag1aSzeRR1xK3HRvXQ=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "kNvEUNOKv7qz7IX3M9Qusw==:YHE7W0qbJzwaCM+FoC1fAKpZ7/HxWcPuB8DHQipo9GM=");
        }
    }
}
