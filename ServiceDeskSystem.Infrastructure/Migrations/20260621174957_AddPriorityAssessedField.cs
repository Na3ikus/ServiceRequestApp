using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityAssessedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPriorityAssessed",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 10,
                column: "IsPriorityAssessed",
                value: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 15,
                column: "IsPriorityAssessed",
                value: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPriorityAssessed",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "3nf9LZKd77y1N+MIuviUQg==:KF7f6nR2+FjYy8yaKw7YLtvKREf1eSXRnPbd29JkfKU=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "+DAdxYwknBeBMbzQScDLyw==:dOYyleCb8ob7SSOW8ed6l+xIm5rmv1lVCDmBDpuZJ0w=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "rRxhNPGMY+MC00h1e5Gltw==:QtpRAn8+ETbPRrek5XvdvKCQVwrDRQseoPNJraRzAOA=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "kMtaCLckiN+pWNgSopMNPQ==:letxNFAOp/FyBi78DXpYj5wz/1Rvei03Z+u6ZtTSwdQ=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "aQEIQi87vUh5DDAXy7Hemg==:6+rSb8NQTYe0/mHjT2I6rvEiE+6zdJLyiYWGwtxkm8w=");
        }
    }
}
