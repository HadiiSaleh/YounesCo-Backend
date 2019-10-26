using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YounesCo_Backend.Migrations
{
    public partial class firstUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestedOn",
                table: "Orders",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 100);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestedOn",
                table: "Orders",
                type: "datetime2",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
