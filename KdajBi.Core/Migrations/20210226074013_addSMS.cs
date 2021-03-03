using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addSMS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsCampaigns",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(nullable: false),
                    CompanyId = table.Column<long>(nullable: false),
                    Guid = table.Column<Guid>(nullable: false, defaultValueSql: "newId()"),
                    Date = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    MsgTxt = table.Column<string>(maxLength: 640, nullable: false),
                    SendAfter = table.Column<DateTime>(nullable: true, defaultValueSql: "GETDATE()"),
                    ApprovedAt = table.Column<DateTime>(nullable: true),
                    CanceledAt = table.Column<DateTime>(nullable: true),
                    SentAt = table.Column<DateTime>(nullable: true),
                    RecipientsCount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsCampaigns_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_SmsCampaigns_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "SmsMsgs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Recipient = table.Column<string>(maxLength: 15, nullable: false),
                    ApiResponse = table.Column<string>(maxLength: 100, nullable: true),
                    SmsCampaignId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMsgs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsMsgs_SmsCampaigns_SmsCampaignId",
                        column: x => x.SmsCampaignId,
                        principalTable: "SmsCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsCampaigns_CompanyId",
                table: "SmsCampaigns",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsCampaigns_UserID",
                table: "SmsCampaigns",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMsgs_SmsCampaignId",
                table: "SmsMsgs",
                column: "SmsCampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsMsgs");

            migrationBuilder.DropTable(
                name: "SmsCampaigns");
        }
    }
}
