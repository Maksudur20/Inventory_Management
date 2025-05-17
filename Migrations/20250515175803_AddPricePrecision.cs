using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPricePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "StockMovements",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "StockMovements",
                newName: "MovementDate");

            migrationBuilder.RenameColumn(
                name: "QuantityChanged",
                table: "StockMovements",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Items",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "StockMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StockMovements",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Items",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Items",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "Items",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_AspNetUsers_UserId",
                table: "StockMovements",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_AspNetUsers_UserId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "StockMovements",
                newName: "QuantityChanged");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "StockMovements",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "MovementDate",
                table: "StockMovements",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Items",
                newName: "LastUpdated");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Items",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
