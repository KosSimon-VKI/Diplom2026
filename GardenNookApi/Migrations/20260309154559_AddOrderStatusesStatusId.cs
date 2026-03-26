using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GardenNookApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusesStatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "В процессе" },
                    { 2, "Отменен" }
                });

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE [Orders]
SET [StatusID] = CASE
    WHEN [Status] IS NULL THEN 1
    WHEN [Status] LIKE N'Отмен%' THEN 2
    ELSE 1
END
");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StatusID",
                table: "Orders",
                column: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStatuses",
                table: "Orders",
                column: "StatusID",
                principalTable: "OrderStatuses",
                principalColumn: "Id");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE o
SET o.[Status] = s.[Name]
FROM [Orders] o
LEFT JOIN [OrderStatuses] s ON s.[Id] = o.[StatusID]
");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStatuses",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StatusID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "OrderStatuses");
        }
    }
}
