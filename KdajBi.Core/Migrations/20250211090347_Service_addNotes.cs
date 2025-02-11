using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class Service_addNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Services",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Services");
        }
    }
}
