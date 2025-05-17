using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialItemData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "Name", "Description", "Price", "Quantity", "SKU", "ReorderLevel", "CreatedAt" },
                values: new object[] { "Sample Item", "Seeded item for initial stock movement.", 10.0m, 100, "SAMPLE-001", 10, System.DateTime.Now }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "SKU",
                keyValue: "SAMPLE-001"
            );
        }
    }
}
