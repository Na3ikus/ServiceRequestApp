using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstRespondedAt",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsResponseSlaBreached",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDueAt",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlaPolicyId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SlaPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ResponseMinutes = table.Column<int>(type: "int", nullable: false),
                    ResolutionMinutes = table.Column<int>(type: "int", nullable: false),
                    WarningThresholdPercent = table.Column<int>(type: "int", nullable: false),
                    EscalateOnBreach = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaPolicies_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "SlaPolicies",
                columns: new[] { "Id", "CreatedAt", "Description", "EscalateOnBreach", "IsActive", "Name", "Priority", "ProductId", "ResolutionMinutes", "ResponseMinutes", "UpdatedAt", "WarningThresholdPercent" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Global fallback policy for critical incidents.", true, true, "Critical — default", "Critical", null, 240, 60, null, 75 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Global fallback policy for high priority tickets.", true, true, "High — default", "High", null, 2880, 240, null, 80 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Global fallback policy for medium priority tickets.", false, true, "Medium — default", "Medium", null, 7200, 480, null, 80 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Global fallback policy for low priority tickets.", false, true, "Low — default", "Low", null, 20160, 1440, null, 90 }
                });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "FirstRespondedAt", "IsResponseSlaBreached", "ResponseDueAt", "SlaPolicyId" },
                values: new object[] { null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "Bx8lP0xcJC+39VZ74hJQww==:f0/ad7/0TSy7RtEaYQLpFrR4QWyixXutOxjfjh+rD88=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "S6VqSpcr2y2l7M9oZ6lgfw==:hYr1UkAbgChdm6/xJeqiF0CgObBMjW157asfk2gTaJ4=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "lmWVVi6W2MdvmRvilcUDog==:3wsoEtGxAFLQUv+/kPZ55PZUqGHVsU8qM8ELBrJEF0U=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "lZ8FkJGw6XTig+/IrSL2pg==:6fEe8q2YsRsGzKT8Z+k/MDofAgevBz4I2lPMT+xegu8=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "m8kjROFeIS6KmTIOT9kInA==:3FW2bPtJQP89nbn9LLYp0EiH7qA20mBlKwtFpWi4Ses=");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SlaPolicyId",
                table: "Tickets",
                column: "SlaPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status_ResponseDueAt",
                table: "Tickets",
                columns: new[] { "Status", "ResponseDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SlaPolicies_Priority_ProductId",
                table: "SlaPolicies",
                columns: new[] { "Priority", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaPolicies_ProductId",
                table: "SlaPolicies",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_SlaPolicies_SlaPolicyId",
                table: "Tickets",
                column: "SlaPolicyId",
                principalTable: "SlaPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_SlaPolicies_SlaPolicyId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "SlaPolicies");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SlaPolicyId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status_ResponseDueAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FirstRespondedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsResponseSlaBreached",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ResponseDueAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SlaPolicyId",
                table: "Tickets");

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
        }
    }
}
