using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NameTagsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketTags",
                columns: table => new
                {
                    TagsId = table.Column<int>(type: "int", nullable: false),
                    TicketsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTags", x => new { x.TagsId, x.TicketsId });
                    table.ForeignKey(
                        name: "FK_TicketTags_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTags_Tickets_TicketsId",
                        column: x => x.TicketsId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "CreatedById", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "#EF4444", null, null, "Bug" },
                    { 2, "#8B5CF6", null, null, "UI/UX" },
                    { 3, "#3B82F6", null, null, "Backend" },
                    { 4, "#F59E0B", null, null, "Urgent" },
                    { 5, "#10B981", null, null, "Feature" },
                    { 6, "#EC4899", null, null, "Security" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "pC0sSSR88nI5hTn1ba+few==:xfY4L0qMCR0dWDni4B04beW1duCsXChXeZ3/RVvnAKk=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "Y99qaHCukyyM2In6oXB5zg==:Qcm4+rcsPnTz3J2yaWVFcZ1oef5p8Xj5eF5oOkUagWk=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "pB/BjVEYWvYtqdKsL41yBQ==:1Zl4ikOStt18JxHlYla8YEupJby2D+x8TLUUjtJVJs8=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "iPiQKNloUxX1wavTyWMtKA==:T6h/6odLJgNesHmsOppsInphQmzIAH/fPRHq/eJfsCM=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "GW3PaXn4JDpByhsCxisUdw==:1BvMg19242nYQg1fT3dzGq5mkyf3GMKoVHtkUXHh33U=");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CreatedById",
                table: "Tags",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTags_TicketsId",
                table: "TicketTags",
                column: "TicketsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "dt/ylVxnzrxSoV06M5k35Q==:1eYeWTFkvs5+eOMbdIXiXN/OPWzKzfa29Nl1yKXm/f8=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "jw3UYcLHggFpPkNN5uPfAQ==:aXdmGsjnFb5bQ3wWmYeoRU6cHxrrflVZscqyNrRV+/g=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "7eTtWNEwgh7n7f0Vt5De0A==:doKwhhr7yUrwnaPj9JzdRQPGfWkE1G4ZnNV3aiRjZSg=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "Y6v/KdObGtxavxGIAw12Gw==:P7hUl7FEZ42g5Kh8abeBJ7qm+JEZ6HycXO6OqodROLg=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "fyliyjkYoO34hfJmfB2aHA==:TqNXiZrfcTfA23QGb3ohLz8eJzyQC+1DGZE1MIYlfb0=");
        }
    }
}
