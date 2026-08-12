using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryOptimizationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Tickets",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AuthorId_CreatedAt",
                table: "Tickets",
                columns: new[] { "AuthorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DeveloperId_CreatedAt",
                table: "Tickets",
                columns: new[] { "DeveloperId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DeveloperId_Status",
                table: "Tickets",
                columns: new[] { "DeveloperId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status_DueDate",
                table: "Tickets",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Type",
                table: "Tickets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "RecipientUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TicketId_CreatedAt",
                table: "Comments",
                columns: new[] { "TicketId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_AuthorId_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DeveloperId_CreatedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DeveloperId_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status_DueDate",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Type",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TicketId_CreatedAt",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Tickets",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "4h3kKJ5IfHWD3UhFj7dOGA==:WyyYQwsYPy//2sLonxNrwkz5TvYZ6tAZBSq21o1W4aY=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "bc1QjspvMSNW/qJ315kVfw==:xr2KczAVhVF9uGOrLvd4gwFUQelXhkZ6mTz7OTErlQk=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "P/BzIDQsLoonU0MWhFX8YA==:EpqoLhECNQjbT9ZiwxSO5FtclDg9Myk+wRREh6vfBGM=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "VUTiO6i3DPu088vp54Ai5Q==:Qxei+3zWH9OzxqixMINz8oHvCEuZnKaWdEYya+yytrE=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "H9dgm+d/6toOGcKKSKLw4A==:lbLOHesCMRXzdH28FsFgcxdKu62lyUdTlpi8O8wFK9g=");
        }
    }
}
