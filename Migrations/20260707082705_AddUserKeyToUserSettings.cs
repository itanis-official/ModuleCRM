using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModuleCRM.Migrations
{
    /// <inheritdoc />
    public partial class AddUserKeyToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings");

            migrationBuilder.AddColumn<string>(
                name: "UserKey",
                table: "UserSettings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserKey",
                table: "UserSettings",
                column: "UserKey",
                unique: true,
                filter: "[UserKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings");

            migrationBuilder.DropIndex(
                name: "IX_UserSettings_UserKey",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "UserKey",
                table: "UserSettings");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }
    }
}
