using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenNookApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDrinkItemModifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDrinkItemModifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDrinkItemId = table.Column<int>(type: "int", nullable: false),
                    MilkIngredientId = table.Column<int>(type: "int", nullable: true),
                    CoffeeIngredientId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDrinkItemModifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDrinkItemModifiers_Ingredients_Coffee",
                        column: x => x.CoffeeIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderDrinkItemModifiers_Ingredients_Milk",
                        column: x => x.MilkIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderDrinkItemModifiers_OrderDrinkItems",
                        column: x => x.OrderDrinkItemId,
                        principalTable: "OrderDrinkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDrinkItemModifiers_CoffeeIngredientId",
                table: "OrderDrinkItemModifiers",
                column: "CoffeeIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDrinkItemModifiers_MilkIngredientId",
                table: "OrderDrinkItemModifiers",
                column: "MilkIngredientId");

            migrationBuilder.CreateIndex(
                name: "UX_OrderDrinkItemModifiers_OrderDrinkItemId",
                table: "OrderDrinkItemModifiers",
                column: "OrderDrinkItemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDrinkItemModifiers");
        }
    }
}
