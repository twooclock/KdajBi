using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class smsCampaignIdUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsCampaigns_AspNetUsers_UserID",
                table: "SmsCampaigns");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "SmsCampaigns",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SmsCampaigns_UserID",
                table: "SmsCampaigns",
                newName: "IX_SmsCampaigns_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsCampaigns_AspNetUsers_UserId",
                table: "SmsCampaigns",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsCampaigns_AspNetUsers_UserId",
                table: "SmsCampaigns");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SmsCampaigns",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_SmsCampaigns_UserId",
                table: "SmsCampaigns",
                newName: "IX_SmsCampaigns_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsCampaigns_AspNetUsers_UserID",
                table: "SmsCampaigns",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
