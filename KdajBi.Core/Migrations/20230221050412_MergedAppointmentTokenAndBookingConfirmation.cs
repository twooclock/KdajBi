using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class MergedAppointmentTokenAndBookingConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingConfirmations");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "PublicBookings");

            migrationBuilder.AddColumn<DateTime>(
                name: "BookingCreated",
                table: "AppointmentTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "AppointmentTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GCalId",
                table: "AppointmentTokens",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "AppointmentTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Status",
                table: "AppointmentTokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingCreated",
                table: "AppointmentTokens");

            migrationBuilder.DropColumn(
                name: "End",
                table: "AppointmentTokens");

            migrationBuilder.DropColumn(
                name: "GCalId",
                table: "AppointmentTokens");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "AppointmentTokens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AppointmentTokens");

            migrationBuilder.AddColumn<long>(
                name: "AppUserId",
                table: "PublicBookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingConfirmations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Active = table.Column<bool>(type: "bit", nullable: true),
                    AppointmentTokenId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUserID = table.Column<int>(type: "int", nullable: true),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GCalId = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingConfirmations_AppointmentTokens_AppointmentTokenId",
                        column: x => x.AppointmentTokenId,
                        principalTable: "AppointmentTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingConfirmations_AppointmentTokenId",
                table: "BookingConfirmations",
                column: "AppointmentTokenId");
        }
    }
}
