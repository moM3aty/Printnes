using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Printnes.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadyProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReadyProduct",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadyProduct",
                table: "Products");
        }
    }
}
