using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceDeskSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSlaBreached",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SlaWarningSent",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "IsSlaBreached", "SlaWarningSent" },
                values: new object[] { false, false });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSlaBreached",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SlaWarningSent",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "BjukWInCuMaH/mErzEZKog==:q5ZHPF163/aCuaHa64IzYKbhQf3BCg4KWAN/SiNASeU=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "tzxdidPtAjexulZ7GlGUPA==:Un6RFelR+3UHogN4jw2joaJ6/FHLMWQCdaz9zCXeei0=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "CtQ/ks0eLXr8JEWTgkB0GA==:mq743OyiUBBLHM8dvgdaX3WShmC2tY6juW2j7h9Bm7U=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "XeU109GfA+4TcezRB1Yrlw==:1UtNpkF0Fad6J9qV6lmD53sUkSY/HcPCSr1UhcLlY0c=");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "rKtPEDeDr92xuJsbRo3akQ==:SGWA/UYGakTQFl/1XYOh66haf17BUKnl2rt2w9uiXSE=");
        }
    }
}
