using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addGCalIdToBookingConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GCalId",
                table: "BookingConfirmations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GCalId",
                table: "BookingConfirmations");
        }
    }
}
