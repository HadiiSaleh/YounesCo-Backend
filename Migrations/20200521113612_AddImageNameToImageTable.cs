using Microsoft.EntityFrameworkCore.Migrations;

namespace YounesCo_Backend.Migrations
{
    public partial class AddImageNameToImageTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Images",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Images");
        }
    }
}
