using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetUrlToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetUrl",
                table: "kumarcapstone_Notifications",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetUrl",
                table: "kumarcapstone_Notifications");
        }
    }
}
