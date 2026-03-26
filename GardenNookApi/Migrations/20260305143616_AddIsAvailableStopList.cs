using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenNookApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAvailableStopList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "ToppingsAndSyrups",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Drinks",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Dishes",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "ToppingsAndSyrups");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Dishes");
        }
    }
}
