using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MadeByMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4f0925e4-24e9-444a-9f83-f43e22204861", "15c01701-90f9-4498-8204-0dda3f8089eb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7ce53119-50ab-46e8-bad6-317e9609ff13", "43a02433-a2dd-4fb1-b682-846d8053237b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "04c2d287-49eb-4d6d-be92-5231b0bf81a1", "2e990584-f384-4156-ad70-3d5539fd6371" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "63566ece-81d4-4015-9352-d4cf2f321be4", "4fe1fb91-3bf7-4a85-8174-38fa9b0b21aa" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "893bc5a2-9a65-4fc4-981f-08aa7538067b", "f15a4419-b98a-484c-95b7-03e636577a04" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "edecaf7c-4f31-4f9f-b291-e91d3750daea", "9aca95ae-018d-4472-a2e4-add6ea6f00f9" });
        }
    }
}
