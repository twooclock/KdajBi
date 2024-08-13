using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class addServiceOffset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Offset",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Offset",
                table: "Services");
        }
    }
}
