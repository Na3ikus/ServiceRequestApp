using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConstTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Tickets",
                type: "longtext",
                nullable: false,
                defaultValue: "Support")
                .Annotation("MySql:CharSet", "utf8mb4");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "2krM4wHocPJA9k8djg9BWw==:kevhEwysdVb+nEqXvjlwtDjACUSat9gGmKoGetChMs4=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "SLLppWHSPH44sH0ZhLzzxg==:x3csNY8X47tafLbiSyDWhJrjuBz4dfKBVoV5HBqojQ8=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "Q5eCoRItjmH0GwJ3ku/wWQ==:ojRS41CUAua93WwEUt1O2Gxy85szA7ZLVhv0oLhCDI4=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "54MmUCTRk8iQIESlEJPmPA==:IfZST1L+godzaskfN/dlUJ1wj5K2d1YAp6QBqlzj3Ds=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "aNFzbK/+A1RpLGktGka8RA==:kf/rpga9O+6kDnYu9ObM6z38s1rqZlp5vfQniUEzMGo=");
        }
    }
}
