using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticalNoteToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnalyticalNote",
                table: "Tickets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "OCkm03VnBqI6moSyb5/dcg==:a06v1iBt7VCudtxJTMb1p7mtj11DTzVNMrBkRQDOTZw=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "43W4T5TUKur3zybF7Dlk+A==:eig7PQtvUDWhzlLEeiGsZ4QvOmRgzFj+g6l66tNjUAM=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "OpnIlYr6Vs+UgGaOouL6OA==:LOkZENdkNuUqLdzp0nYntR5rpbBUtgmH/PXEU5e+cMw=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "BYvz9Hv6W+KmQna0OVqKRg==:HkNzpbx5wolu3cUsXvmJtnlbjejkk3k3KT1jFjtsbOc=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "X2RRPCZaxhiEDHguvwspaw==:5VD8NMW6HhDn0Gy9tN5N/Q8JeRCRSdeRm+kMTLZw8Hk=");

            migrationBuilder.DropColumn(
                name: "AnalyticalNote",
                table: "Tickets");
        }
    }
}
