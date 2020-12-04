using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Tel",
                table: "Locations",
                maxLength: 15,
                nullable: true,
                defaultValue:"",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<long>(
                name: "ScheduleId",
                table: "Locations",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedUserID = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedUserID = table.Column<int>(nullable: true),
                    Active = table.Column<bool>(nullable: true, defaultValueSql: "1"),
                    Type = table.Column<long>(nullable: false, defaultValueSql: "0"),
                    MondayStart = table.Column<DateTime>(nullable: false),
                    MondayEnd = table.Column<DateTime>(nullable: false),
                    TuesdayStart = table.Column<DateTime>(nullable: false),
                    TuesdayEnd = table.Column<DateTime>(nullable: false),
                    WednesdayStart = table.Column<DateTime>(nullable: false),
                    WednesdayEnd = table.Column<DateTime>(nullable: false),
                    ThursdayStart = table.Column<DateTime>(nullable: false),
                    ThursdayEnd = table.Column<DateTime>(nullable: false),
                    FridayStart = table.Column<DateTime>(nullable: false),
                    FridayEnd = table.Column<DateTime>(nullable: false),
                    SaturdayStart = table.Column<DateTime>(nullable: false),
                    SaturdayEnd = table.Column<DateTime>(nullable: false),
                    SundayStart = table.Column<DateTime>(nullable: false),
                    SundayEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ScheduleId",
                table: "Locations",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Schedules_ScheduleId",
                table: "Locations",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Schedules_ScheduleId",
                table: "Locations");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Locations_ScheduleId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "Tel",
                table: "Locations",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
