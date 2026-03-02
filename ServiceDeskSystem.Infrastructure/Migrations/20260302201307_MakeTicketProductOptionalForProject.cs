using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeTicketProductOptionalForProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                column: "Type",
                value: "Support");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 5,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 6,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 7,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 8,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 9,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 10,
                column: "Type",
                value: "Bug");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 15,
                column: "Type",
                value: "Project");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 5,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 6,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 7,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 8,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 9,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 10,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 15,
                column: "Type",
                value: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "zLmaYVUr+qteJF+dCyl92g==:wYnbkJ0KYKV0AtftHau5jlWl9AkrYMSEUsd2p+qyuZw=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "1X8jQfnUKpIDN2Yh+Y2ecA==:35A6JcwRe3mnrQPLpz/L9o2uE7E+8LaAOqejB+IQNfE=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "vxgaKmod5l0XW9aoYLF6Qw==:X8dFbDCbJOY8VOVBf5zjDuiP6csC4H6FHkKbSwZcbPs=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "SoelECj5lSjCqJXV8LTarw==:oZWJunpQqb3rvxjRKwbYrMM1wN3L4TfKs+mNw4IYlns=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "7oOaRxKY+/r2j3Z8pEny7w==:EYYPkSQpAJj47PHh3uEI4Tt3vitkc8Mmai2Ekvs9Fkc=");
        }
    }
}
