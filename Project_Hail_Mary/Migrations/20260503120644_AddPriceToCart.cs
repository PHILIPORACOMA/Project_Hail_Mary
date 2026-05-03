using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project_Hail_Mary.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Cart",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Cart");
        }
    }
}
