using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Migrations
{
    /// <inheritdoc />
    public partial class DropCategoryUniqueDisplayOrderIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "CATEGORY_UNIQUE_DISPLAYORDER",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "CATEGORY_UNIQUE_DISPLAYORDER",
                table: "Categories",
                column: "DisplayOrder",
                unique: true);
        }
    }
}
