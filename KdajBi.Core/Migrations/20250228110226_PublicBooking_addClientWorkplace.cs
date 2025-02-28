using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class PublicBooking_addClientWorkplace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClientWorkplaceId",
                table: "PublicBookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookings_ClientWorkplaceId",
                table: "PublicBookings",
                column: "ClientWorkplaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicBookings_Workplaces_ClientWorkplaceId",
                table: "PublicBookings",
                column: "ClientWorkplaceId",
                principalTable: "Workplaces",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicBookings_Workplaces_ClientWorkplaceId",
                table: "PublicBookings");

            migrationBuilder.DropIndex(
                name: "IX_PublicBookings_ClientWorkplaceId",
                table: "PublicBookings");

            migrationBuilder.DropColumn(
                name: "ClientWorkplaceId",
                table: "PublicBookings");
        }
    }
}
