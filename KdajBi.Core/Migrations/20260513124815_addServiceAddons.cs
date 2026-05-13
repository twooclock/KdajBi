using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KdajBi.Core.Migrations
{
    public partial class addServiceAddons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceAddons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Minutes = table.Column<int>(type: "int", nullable: false),
                    PriceDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "''"),
                    Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true, defaultValueSql: "''"),
                    CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedUserID = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedUserID = table.Column<int>(nullable: true),
                    Active = table.Column<bool>(nullable: true, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAddons", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceAddons");
        }
    }
}
