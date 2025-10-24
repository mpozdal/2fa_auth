using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _2FAService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTwoFactorSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EncryptedSecretKey = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTwoFactorSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRecoveryCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HashedCode = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UserTwoFactorSettingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecoveryCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecoveryCodes_UserTwoFactorSettings_UserTwoFactorSettin~",
                        column: x => x.UserTwoFactorSettingId,
                        principalTable: "UserTwoFactorSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRecoveryCodes_UserTwoFactorSettingId",
                table: "UserRecoveryCodes",
                column: "UserTwoFactorSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTwoFactorSettings_UserId",
                table: "UserTwoFactorSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRecoveryCodes");

            migrationBuilder.DropTable(
                name: "UserTwoFactorSettings");
        }
    }
}
