using Microsoft.EntityFrameworkCore.Migrations;

namespace KdajBi.Core.Migrations
{
    public partial class SeedFirstUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT AspNetRoles ON
                INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (1, N'Admin', N'Admin', N'dd0bb2c2-f958-4cfe-bbf9-43f37773bd80');
                SET IDENTITY_INSERT AspNetRoles OFF
                SET IDENTITY_INSERT AspNetUsers ON
                INSERT INTO [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FirstName], [LastName]) VALUES (1, N'admin@ellamon.local', N'ADMIN@ELLAMON.LOCAL', N'admin@ellamon.local', N'ADMIN@ELLAMON.LOCAL', 0, N'AQAAAAEAACcQAAAAEKrv+eI6dB0HcBSfJR/Aqaq3ivq/jxO0oe3QVc1ozHSONoLb40jWFb2AkkefWc81YQ==', N'CANMASWX5N6RJACKZGXHCRANMVSFTTJZ', N'dd0bb2c2-f958-4cfe-bbf9-43f37773bd80', NULL, 0, 0, NULL, 1, 0, NULL, NULL);
                SET IDENTITY_INSERT AspNetUsers OFF
                INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 1);
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM [dbo].[AspNetUserRoles] where [UserId]=1 and [RoleId]=1;
                DELETE FROM [dbo].[AspNetRoles] where ID=1;
                DELETE FROM [dbo].[AspNetUsers] where ID=1;
                ");
        }
    }
}
