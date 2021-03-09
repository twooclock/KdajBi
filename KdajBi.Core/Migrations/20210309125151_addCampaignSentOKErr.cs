using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addCampaignSentOKErr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SentError",
                table: "SmsCampaigns",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SentOk",
                table: "SmsCampaigns",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentError",
                table: "SmsCampaigns");

            migrationBuilder.DropColumn(
                name: "SentOk",
                table: "SmsCampaigns");
        }
    }
}
