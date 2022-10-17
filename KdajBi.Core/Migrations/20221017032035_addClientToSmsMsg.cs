using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class addClientToSmsMsg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ClientId",
                table: "SmsMsgs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_SmsMsgs_ClientId",
                table: "SmsMsgs",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsMsgs_Clients_ClientId",
                table: "SmsMsgs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsMsgs_Clients_ClientId",
                table: "SmsMsgs");

            migrationBuilder.DropIndex(
                name: "IX_SmsMsgs_ClientId",
                table: "SmsMsgs");

            migrationBuilder.AlterColumn<long>(
                name: "ClientId",
                table: "SmsMsgs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
