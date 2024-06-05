using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class ServicePriceDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriceDescription",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true, defaultValueSql: "''");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceGroupId",
                table: "Services",
                column: "ServiceGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ServiceGroups_ServiceGroupId",
                table: "Services",
                column: "ServiceGroupId",
                principalTable: "ServiceGroups",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ServiceGroups_ServiceGroupId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ServiceGroupId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "PriceDescription",
                table: "Services");
        }
    }
}
