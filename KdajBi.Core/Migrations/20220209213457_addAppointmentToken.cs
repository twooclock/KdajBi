using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addAppointmentToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentTokens",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    CreatedUserID = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    UpdatedUserID = table.Column<int>(nullable: true),
                    Active = table.Column<bool>(nullable: true),
                    Token = table.Column<string>(nullable: false),
                    Service = table.Column<string>(nullable: false),
                    Minutes = table.Column<long>(nullable: false),
                    CompanyId = table.Column<long>(nullable: false),
                    LocationId = table.Column<long>(nullable: false),
                    ClientId = table.Column<long>(nullable: false),
                    AppUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTokens", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentTokens");
        }
    }
}
