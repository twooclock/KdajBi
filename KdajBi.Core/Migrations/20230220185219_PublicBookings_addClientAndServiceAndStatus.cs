using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class PublicBookings_addClientAndServiceAndStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClientId",
                table: "PublicBookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ServiceId",
                table: "PublicBookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Status",
                table: "PublicBookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookings_ClientId",
                table: "PublicBookings",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookings_ServiceId",
                table: "PublicBookings",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicBookings_Clients_ClientId",
                table: "PublicBookings",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicBookings_Services_ServiceId",
                table: "PublicBookings",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicBookings_Clients_ClientId",
                table: "PublicBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicBookings_Services_ServiceId",
                table: "PublicBookings");

            migrationBuilder.DropIndex(
                name: "IX_PublicBookings_ClientId",
                table: "PublicBookings");

            migrationBuilder.DropIndex(
                name: "IX_PublicBookings_ServiceId",
                table: "PublicBookings");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "PublicBookings");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "PublicBookings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PublicBookings");
        }
    }
}
