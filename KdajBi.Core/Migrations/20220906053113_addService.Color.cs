using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addServiceColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Services",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Services");
        }
    }
}
