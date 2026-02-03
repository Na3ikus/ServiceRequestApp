using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServiceDeskSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 14);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "AffectedVersion", "AuthorId", "CreatedAt", "Description", "DeveloperId", "Environment", "Priority", "ProductId", "Status", "StepsToReproduce", "Title" },
                values: new object[,]
                {
                    { 11, "1.4.2", 4, new DateTime(2024, 4, 8, 7, 30, 0, 0, DateTimeKind.Utc), "The receipt printer loses connection randomly during operation, requiring terminal restart.", 5, "POS Terminal Hardware v2", "Critical", 6, "Code Review", "1. Process several transactions\n2. After ~20-30 receipts, printer stops responding", "Printer connection lost randomly" },
                    { 12, "2.1.0", 3, new DateTime(2024, 4, 22, 8, 0, 0, 0, DateTimeKind.Utc), "Система контролю доступу не реагує на NFC картки після останнього оновлення прошивки.", 5, "Smart Lock Hardware v3", "Critical", 7, "In Progress", "1. Піднести NFC картку до зчитувача\n2. Очікувати відкриття замка\n3. Замок не реагує", "Не працює відкриття через NFC" },
                    { 13, "5.0.1", 4, new DateTime(2024, 4, 14, 15, 0, 0, 0, DateTimeKind.Utc), "VPN connections are dropping when more than 50 concurrent users are connected.", 2, "Office Router Pro Hardware", "High", 8, "Testing", "1. Establish 50+ VPN connections\n2. Generate network traffic\n3. Monitor connection stability", "VPN tunnel drops under heavy load" },
                    { 14, "5.0.0", 4, new DateTime(2024, 3, 20, 10, 0, 0, 0, DateTimeKind.Utc), "New firewall rules added through web interface are not being applied until device restart.", 5, "Office Router Pro Hardware", "Medium", 8, "Done", "1. Login to web interface\n2. Add new firewall rule\n3. Save configuration\n4. Test rule - not working", "Firewall rules not applying" }
                });
        }
    }
}
