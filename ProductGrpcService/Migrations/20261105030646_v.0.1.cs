using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductGrpcService.Migrations
{
    /// <inheritdoc />
    public partial class v01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sample_Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sample_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sample_Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    WarehouseLocation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sample_Inventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sample_Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sample_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "sample_Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Computer peripherals and accessories", "Accessories" },
                    { 2, "Mechanical and membrane keyboards", "Keyboards" }
                });

            migrationBuilder.InsertData(
                table: "sample_Inventories",
                columns: new[] { "Id", "ProductId", "StockQuantity", "WarehouseLocation" },
                values: new object[] { 1, 1, 500, "Main Warehouse" });

            migrationBuilder.InsertData(
                table: "sample_Products",
                columns: new[] { "Id", "CategoryId", "Description", "InventoryId", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "High precision RGB mouse", 1, "Wireless Gaming Mouse", 59 },
                    { 2, 2, "Blue switch tactile keyboard", 1, "Mechanical Keyboard", 89 },
                    { 3, 1, "34-inch curved display", 1, "UltraWide 4K Monitor", 399 },
                    { 4, 1, "Over-ear wireless audio", 1, "Noise-Cancelling Headset", 149 },
                    { 5, 1, "7-in-1 adapter dongle", 1, "USB-C Hub Multiport", 29 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sample_Categories");

            migrationBuilder.DropTable(
                name: "sample_Inventories");

            migrationBuilder.DropTable(
                name: "sample_Products");
        }
    }
}
