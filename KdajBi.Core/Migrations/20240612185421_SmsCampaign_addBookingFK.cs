using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class SmsCampaign_addBookingFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AppointmentTokenId",
                table: "SmsCampaigns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PublicBookingId",
                table: "SmsCampaigns",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentTokenId",
                table: "SmsCampaigns");

            migrationBuilder.DropColumn(
                name: "PublicBookingId",
                table: "SmsCampaigns");
        }
    }
}
