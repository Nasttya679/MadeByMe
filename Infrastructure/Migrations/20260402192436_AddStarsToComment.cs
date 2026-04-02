using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadeByMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStarsToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stars",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "5961697f-5534-412c-8ef5-3fdeae62dfc1", "90341869-f75a-410d-a40a-e0dd59c661af" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6adc414e-1f36-4723-9b87-ac9d099bdfa9", "a3ceff90-e1f0-4779-a8f0-c37ad733067b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2d64985e-737a-4fb7-a597-4aa8bfae4b95", "2490ae3c-4844-4439-99b5-585e3f573a4f" });

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: 1,
                column: "Stars",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Comments",
                keyColumn: "CommentId",
                keyValue: 2,
                column: "Stars",
                value: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stars",
                table: "Comments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2cbe1a8f-11ac-49ab-a61e-f93c216ab2ce", "f7985b24-2b5f-4eb2-8393-98fb269c6a86" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "85e2ed53-d54b-4c73-94f2-2cb7f880ab75", "1eaf60ca-25d5-4ca6-a13d-19b088443ae1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "9bdcfebd-914f-4ab9-814a-b61a1d00434b", "0c09abf6-e4a6-4457-b12a-ccd799eb7d68" });
        }
    }
}
