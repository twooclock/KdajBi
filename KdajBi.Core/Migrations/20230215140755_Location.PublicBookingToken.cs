using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class LocationPublicBookingToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicBookingToken",
                table: "Locations",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                defaultValueSql: "''");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_PublicBookingToken",
                table: "Locations",
                column: "PublicBookingToken",
                unique: true,
                filter: "([PublicBookingToken] IS NOT NULL) AND ([PublicBookingToken] != '')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_PublicBookingToken",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "PublicBookingToken",
                table: "Locations");
        }
    }
}
