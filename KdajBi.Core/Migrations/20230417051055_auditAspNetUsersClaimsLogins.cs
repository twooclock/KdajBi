using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace KdajBi.Core.Migrations
{
    public partial class auditAspNetUsersClaimsLogins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
               name: "LastChangeDate",
               table: "AspNetUserLogins",
               type: "datetime2",
               nullable: true,
               defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
               name: "LastChangeDate",
               table: "AspNetUserClaims",
               type: "datetime2",
               nullable: true,
               defaultValueSql: "GETDATE()");

            migrationBuilder.Sql(@"CREATE TRIGGER AspNetUserClaims_SetLastChange 
               ON  AspNetUserClaims
               AFTER   UPDATE
            AS
            BEGIN

                SET NOCOUNT ON;

                UPDATE AspNetUserClaims
                SET LastChangeDate = GETDATE()
                WHERE ID IN(SELECT DISTINCT ID FROM Inserted)
            END
            GO");

            migrationBuilder.Sql(@"CREATE TRIGGER AspNetUserLogins_SetLastChange 
               ON  AspNetUserLogins
               AFTER   UPDATE
            AS
            BEGIN

                SET NOCOUNT ON;

                UPDATE AspNetUserLogins
                SET LastChangeDate = GETDATE()
                from inserted as i inner join AspNetUserLogins as t 
		        on i.LoginProvider = t.LoginProvider and i.ProviderKey = t.ProviderKey;
            END
            GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER [AspNetUserClaims_SetLastChange]");
            migrationBuilder.Sql("DROP TRIGGER [AspNetUserLogins_SetLastChange]");
            migrationBuilder.DropColumn(
               name: "LastChangeDate",
               table: "AspNetUserClaims");
            migrationBuilder.DropColumn(
               name: "LastChangeDate",
               table: "AspNetUserLogins");
        }
    }
}
