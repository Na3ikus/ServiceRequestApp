using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StableSeedPasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "U3ZjRGVza1NlZWRTYWx0MQ==:orhXRQyasnY0tbIjWT972XgUcYbQYFJOZKf3kiGWZqc=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "U3ZjRGVza1NlZWRTYWx0Mg==:FkNHdz+zH4OkjeLqfT7fVPpxlPIko9BPjX0oXp5SQiQ=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "U3ZjRGVza1NlZWRTYWx0Mw==:NPGoKnpCuSvcy5UOjkFBuUsfPWfY7Wy9n8HEZZZBA30=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "U3ZjRGVza1NlZWRTYWx0NA==:zggZqFDVV5pvh+rK54pZ0zRivz8AGkR4u4NeVmEdlr4=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "U3ZjRGVza1NlZWRTYWx0NQ==:aluImitgI/5MSw+YERyrtv48OrALZ/MfTegJlKLJfYo=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
