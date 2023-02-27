using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class ServiceUsedInClientBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_ScheduleId",
                table: "Locations");

            migrationBuilder.AddColumn<bool>(
                name: "UsedInClientBooking",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ScheduleId",
                table: "Locations",
                column: "ScheduleId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_ScheduleId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UsedInClientBooking",
                table: "Services");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ScheduleId",
                table: "Locations",
                column: "ScheduleId");
        }
    }
}
