using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkLogsAndAttachmentsMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Attachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "Attachments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Attachments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UploadedById",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    TimeSpentMinutes = table.Column<int>(type: "int", nullable: false),
                    DateLogged = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkLogs_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "j96gqgfFa5fBIxf2O4SsOA==:B5POx/MZbn7F7drfOAZkJ4t9l1ZWST/69Z1vxuuMJYY=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "R9sk3pGPJNIjJlW4MvXWWQ==:HI7h5DpvtE4iPjVJpWTpNxG3fqEmYsCOtQwTHp+s1aQ=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "lyAOoniHhAzntbLu1W2t7g==:whLAw5K0Mk4P1SSi4w8T5f4kdQQAsZ8ChsaFAHx6GzE=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "njvZY9yzn9Md4GA1/jKFjA==:sbAI53wIg66E66sLiMzzfSYKVjGq3DsvIs6obWEGKZk=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "zlt10tCGBThVtTg2UokJxQ==:nmnrm9b9OGAFPLT/+wB7/9asxLcNoNh32lYKgUNC1tQ=");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachments",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploadedById",
                table: "Attachments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_TicketId",
                table: "WorkLogs",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_UserId",
                table: "WorkLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Users_UploadedById",
                table: "Attachments",
                column: "UploadedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Users_UploadedById",
                table: "Attachments");

            migrationBuilder.DropTable(
                name: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_UploadedById",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "UploadedById",
                table: "Attachments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "z2xpFs402rM2/M5njfnz6Q==:zKeFwlrSndc8hHQCU+RmeyFJ7LLBzprE7JoIDAD3kOs=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "FvDZnZWHzCvdPLRiRsNiuw==:04OIU3A5oijuCiLclZLna2eieZnMJaxCbLqaioRazpA=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "NWtqlPMdhnWqhJBvNuJ2Ew==:Mevz3hrBZK/Vg67XYR28dT9HFtBO3A0/g6E8N1gceVU=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "GefccwtxkfBmUZD8B6d8ZA==:gN+i/slso0OUXyX68FPGznICtR9DAl3LKm6df56fia8=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "eKa9JM9ThQMrwLuXbhH1gw==:K9RYsATuq4vzdWS0+zk9txD5i1iP3MQdZR6hABldC54=");
        }
    }
}
