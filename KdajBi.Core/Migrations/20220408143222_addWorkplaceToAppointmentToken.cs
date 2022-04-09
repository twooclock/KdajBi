using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addWorkplaceToAppointmentToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "WorkplaceId",
                table: "AppointmentTokens",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTokens_ClientId",
                table: "AppointmentTokens",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTokens_CompanyId",
                table: "AppointmentTokens",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTokens_LocationId",
                table: "AppointmentTokens",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTokens_WorkplaceId",
                table: "AppointmentTokens",
                column: "WorkplaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTokens_Clients_ClientId",
                table: "AppointmentTokens",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTokens_Companies_CompanyId",
                table: "AppointmentTokens",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTokens_Locations_LocationId",
                table: "AppointmentTokens",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentTokens_Workplaces_WorkplaceId",
                table: "AppointmentTokens",
                column: "WorkplaceId",
                principalTable: "Workplaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTokens_Clients_ClientId",
                table: "AppointmentTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTokens_Companies_CompanyId",
                table: "AppointmentTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTokens_Locations_LocationId",
                table: "AppointmentTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentTokens_Workplaces_WorkplaceId",
                table: "AppointmentTokens");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTokens_ClientId",
                table: "AppointmentTokens");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTokens_CompanyId",
                table: "AppointmentTokens");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTokens_LocationId",
                table: "AppointmentTokens");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentTokens_WorkplaceId",
                table: "AppointmentTokens");

            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "AppointmentTokens");
        }
    }
}
