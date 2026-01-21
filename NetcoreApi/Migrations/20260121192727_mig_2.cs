using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NetcoreApi.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("29d706b2-603d-43a9-a2d2-c229a13eb940"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("74126409-da1b-4b0b-83e5-2e037db44bee"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("7f10ecb1-c95f-4f7b-8b5b-fc60301d4c7b"));

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "Name", "Price", "Stock", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("378bff0d-51d4-4820-81c4-a39cf0f1ca91"), new DateTime(2026, 1, 21, 19, 27, 26, 881, DateTimeKind.Utc).AddTicks(8373), "High-performance laptop", null, "Laptop", 999.99m, 10, null },
                    { new Guid("d3409309-97a1-4174-bd4b-6fc34fb36f60"), new DateTime(2026, 1, 21, 19, 27, 26, 881, DateTimeKind.Utc).AddTicks(8376), "Wireless mouse", null, "Mouse", 29.99m, 50, null },
                    { new Guid("d80e93fb-22ce-409f-b385-432470e0d130"), new DateTime(2026, 1, 21, 19, 27, 26, 881, DateTimeKind.Utc).AddTicks(8377), "Mechanical keyboard", null, "Keyboard", 89.99m, 30, null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 21, 19, 27, 26, 881, DateTimeKind.Utc).AddTicks(8266));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 21, 19, 27, 26, 881, DateTimeKind.Utc).AddTicks(8269));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 21, 19, 27, 26, 881, DateTimeKind.Utc).AddTicks(8270));

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("378bff0d-51d4-4820-81c4-a39cf0f1ca91"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d3409309-97a1-4174-bd4b-6fc34fb36f60"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d80e93fb-22ce-409f-b385-432470e0d130"));

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "Name", "Price", "Stock", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("29d706b2-603d-43a9-a2d2-c229a13eb940"), new DateTime(2026, 1, 19, 18, 16, 8, 553, DateTimeKind.Utc).AddTicks(7792), "High-performance laptop", null, "Laptop", 999.99m, 10, null },
                    { new Guid("74126409-da1b-4b0b-83e5-2e037db44bee"), new DateTime(2026, 1, 19, 18, 16, 8, 553, DateTimeKind.Utc).AddTicks(7795), "Wireless mouse", null, "Mouse", 29.99m, 50, null },
                    { new Guid("7f10ecb1-c95f-4f7b-8b5b-fc60301d4c7b"), new DateTime(2026, 1, 19, 18, 16, 8, 553, DateTimeKind.Utc).AddTicks(7796), "Mechanical keyboard", null, "Keyboard", 89.99m, 30, null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 18, 16, 8, 553, DateTimeKind.Utc).AddTicks(7650));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 18, 16, 8, 553, DateTimeKind.Utc).AddTicks(7654));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 18, 16, 8, 553, DateTimeKind.Utc).AddTicks(7655));
        }
    }
}
