using Microsoft.EntityFrameworkCore.Migrations;

namespace YounesCo_Backend.Migrations
{
    public partial class AddFeaturesToProductsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Features",
                table: "Products");
        }
    }
}
