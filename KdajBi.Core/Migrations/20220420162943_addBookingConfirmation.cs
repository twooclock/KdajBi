using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addBookingConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingConfirmations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    CreatedUserID = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserID = table.Column<int>(nullable: true),
                    Active = table.Column<bool>(nullable: true),
                    AppointmentTokenId = table.Column<long>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingConfirmations");
        }
    }
}
