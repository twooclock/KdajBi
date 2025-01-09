using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class ExtendCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Davcna",
                table: "Companies",
                newName: "TaxVATNumber");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Companies",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxpayer",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Companies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxIDNumber",
                table: "Companies",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Zip",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZipTown",
                table: "Companies",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IsTaxpayer",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TaxIDNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Zip",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ZipTown",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "TaxVATNumber",
                table: "Companies",
                newName: "Davcna");
        }
    }
}
