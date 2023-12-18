using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class addServiceGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ServiceGroupId",
                table: "Services",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    LocationId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortPosition = table.Column<int>(type: "int", nullable: false),
					CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
					CreatedUserID = table.Column<int>(nullable: true),
					UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
					UpdatedUserID = table.Column<int>(nullable: true),
					Active = table.Column<bool>(nullable: true, defaultValueSql: "1")
				},
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGroups", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceGroups");

            migrationBuilder.DropColumn(
                name: "ServiceGroupId",
                table: "Services");
        }
    }
}
