using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProbs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "QeHKeVeVomIvh6+s/K+YkQ==:EE4+iL4KOk521aLlY7SaA4NLm/MiiEALlLwh4VSaVGY=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "7XIihbp63K8YWhQaeO5+3Q==:vntij3slhOmLDNhor1byotMua4eUSnTE7yHlElNrEc8=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "o0lDkxApNQGUpnZb5GYk2w==:4gnpE356/9xU9GyrUaZsHBlr8HwxKKJ/rjiB9nOU6mE=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "rx6F85Eg/w0Gztgcnq5uYg==:wJp+5UnZ+H1ub3vMNAO4SDaHtjNpVYa5PSLaQS/JlFo=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "rq41DD5qehtpkb0w0Wh0TQ==:l8YIwuNK0NH+0oLJX7iYwxrh2q40QdgsGJbY/Qsa83E=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
