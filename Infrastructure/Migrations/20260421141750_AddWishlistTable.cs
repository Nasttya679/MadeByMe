using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MadeByMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wishlists_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp" },
                values: new object[] { "63566ece-81d4-4015-9352-d4cf2f321be4", null, "4fe1fb91-3bf7-4a85-8174-38fa9b0b21aa" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp" },
                values: new object[] { "893bc5a2-9a65-4fc4-981f-08aa7538067b", null, "f15a4419-b98a-484c-95b7-03e636577a04" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp" },
                values: new object[] { "edecaf7c-4f31-4f9f-b291-e91d3750daea", null, "9aca95ae-018d-4472-a2e4-add6ea6f00f9" });

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_PostId",
                table: "Wishlists",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_UserId",
                table: "Wishlists",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp" },
                values: new object[] { "db630b0f-31b5-4931-84ed-e8796a01a7f9", "ADMIN", "4fb4f680-87fd-4063-a0d0-9c958de9350e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp" },
                values: new object[] { "d0a5efdb-1e26-4891-b703-5e52c9cdbb72", "ARTIST123", "c3ee4e42-bf1f-4c61-a4ae-9ff8b2a87809" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-3333-3333-3333-333333333333",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp" },
                values: new object[] { "b575357d-2c1f-4def-b822-5caed3e90d5f", "CUSTOMER1", "2da542c3-3a74-4fb7-adc8-d3099be0fa48" });
        }
    }
}
