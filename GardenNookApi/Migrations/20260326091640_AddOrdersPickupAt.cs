using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenNookApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersPickupAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickupAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickupAt",
                table: "Orders");
        }
    }
}
