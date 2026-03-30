using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Printnes.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_Products_ProductId1",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_ProductId1",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "ProductReviews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "ProductReviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId1",
                table: "ProductReviews",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_Products_ProductId1",
                table: "ProductReviews",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
