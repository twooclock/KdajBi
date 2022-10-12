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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentTokenId = table.Column<long>(type: "bigint", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GCalId = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedUserID = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(nullable: true, defaultValueSql: "1")
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
