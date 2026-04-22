using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadeByMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByUserIdToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f525fdc8-77b0-4dc1-98a0-e86cd1e1d114", "72389312-8ccb-4c1a-9dce-e271d51122c5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "0c81f914-1379-42cc-a067-ac2d368bc1c1", "a9ec0e2b-b8bf-4760-a8fb-23ffb235073a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "db964f37-0deb-4346-8350-ecf73ea4c869", "f0f7c1c8-80f7-4c3f-946e-c8cc42628910" });

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeletedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeletedByUserId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Posts");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "1083953d-30ba-45b9-bf60-a9f727c06db9", "68e7cb31-1d3c-409b-85e1-d2e586d1e635" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2454d5d8-3c38-4b51-9d6e-a7b3c88e4531", "2703c3d7-05cf-47e5-9647-c256bc33430e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3d988e32-47ff-45e8-b0eb-b10e7e077613", "69ffca53-8405-43e6-8a51-9d000c91b54e" });
        }
    }
}
