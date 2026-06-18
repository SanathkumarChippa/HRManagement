using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailAlertsEnabled",
                table: "kumarcapstone_AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InAppNotificationsEnabled",
                table: "kumarcapstone_AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ThemePreference",
                table: "kumarcapstone_AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAlertsEnabled",
                table: "kumarcapstone_AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InAppNotificationsEnabled",
                table: "kumarcapstone_AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ThemePreference",
                table: "kumarcapstone_AspNetUsers");
        }
    }
}
