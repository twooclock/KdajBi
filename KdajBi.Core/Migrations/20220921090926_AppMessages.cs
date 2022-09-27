using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class AppMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForAdminOnly = table.Column<bool>(type: "bit", nullable: false, defaultValueSql: "1"),
                    CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedUserID = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(nullable: true, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAppMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppMessageId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Read = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "0"),
                    DateRead = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedUserID = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(nullable: true, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAppMessages_AppMessages_AppMessageId",
                        column: x => x.AppMessageId,
                        principalTable: "AppMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAppMessages_AppMessageId",
                table: "UserAppMessages",
                column: "AppMessageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAppMessages");

            migrationBuilder.DropTable(
                name: "AppMessages");
        }
    }
}
