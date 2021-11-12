using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class smsMsgsaddCampaignFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsMsgs_SmsCampaigns_SmsCampaignId",
                table: "SmsMsgs");

            migrationBuilder.AlterColumn<long>(
                name: "SmsCampaignId",
                table: "SmsMsgs",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsMsgs_SmsCampaigns_SmsCampaignId",
                table: "SmsMsgs",
                column: "SmsCampaignId",
                principalTable: "SmsCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsMsgs_SmsCampaigns_SmsCampaignId",
                table: "SmsMsgs");

            migrationBuilder.AlterColumn<long>(
                name: "SmsCampaignId",
                table: "SmsMsgs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddForeignKey(
                name: "FK_SmsMsgs_SmsCampaigns_SmsCampaignId",
                table: "SmsMsgs",
                column: "SmsCampaignId",
                principalTable: "SmsCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
