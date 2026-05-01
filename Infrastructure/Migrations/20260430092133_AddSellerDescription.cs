using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadeByMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerDescription",
                table: "AspNetUsers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp", "SellerDescription" },
                values: new object[] { "e9d055e9-7146-416c-8056-78fe4360c788", "92a9e0d9-7441-4544-af6c-cfa9067b4244", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp", "SellerDescription" },
                values: new object[] { "fa372b84-dc6f-410a-b532-ed9f3c1d521d", "1c1d9f76-65c5-49ad-a841-247916ada90e", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp", "SellerDescription" },
                values: new object[] { "19e9ba28-b65e-4ff2-8603-f47e6eb803cb", "827c528d-cf9c-4ae4-8297-50e2bfa5e3ad", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerDescription",
                table: "AspNetUsers");

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
        }
    }
}
