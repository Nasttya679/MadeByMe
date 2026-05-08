using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadeByMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderAvatarToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderAvatar",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4bb3ee39-2d13-4f0d-9acc-56ad58765b3d", "d5703922-a9f2-4c59-9223-db22f0c9f6d5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "fa8985eb-8c4c-4fb4-9de3-c43cf3f57744", "3a1428a8-6614-4b46-ad5d-3b27be7b6a0d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "5b773a46-8dd9-4747-8f0d-09fb5cdfc39c", "a23fdeb7-c261-4508-b7d0-cdba7422df2c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderAvatar",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ae905a89-0024-4581-8797-24e8675fa7b6", "b2159b67-bc9b-4858-b9fa-e1839c17abb8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7636d942-4d51-4a33-aa4b-0cb5e234567c", "2925bd4a-d352-4407-bbe0-2d8b1bf38155" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "8b39a2da-0e76-481f-a576-a8182b8846ee", "5f1da472-c6db-4a96-bbe0-87d07915203e" });
        }
    }
}
