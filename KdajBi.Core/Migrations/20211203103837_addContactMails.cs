using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addContactMails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMails",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromEmail = table.Column<string>(maxLength: 250, nullable: true),
                    FromCompanyId = table.Column<long>(nullable: false),
                    FromLocationId = table.Column<long>(nullable: false),
                    ToEmail = table.Column<string>(maxLength: 250, nullable: true),
                    ToCompanyId = table.Column<long>(nullable: false),
                    ToLocationId = table.Column<long>(nullable: false),
                    Subject = table.Column<string>(maxLength: 150, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    EmailSent = table.Column<bool>(nullable: true, defaultValueSql: "0"),
                    CreatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    CreatedUserID = table.Column<int>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    UpdatedUserID = table.Column<int>(nullable: true),
                    Active = table.Column<bool>(nullable: true, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMails", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMails");
        }
    }
}
