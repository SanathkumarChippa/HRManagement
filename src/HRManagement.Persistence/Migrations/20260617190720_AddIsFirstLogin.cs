using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFirstLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLogin",
                table: "kumarcapstone_AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "kumarcapstone_AspNetUsers");
        }
    }
}
