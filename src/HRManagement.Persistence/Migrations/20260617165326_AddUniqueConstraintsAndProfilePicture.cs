using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsAndProfilePicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "kumarcapstone_Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_kumarcapstone_Employees_Email",
                table: "kumarcapstone_Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kumarcapstone_Departments_Name",
                table: "kumarcapstone_Departments",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_kumarcapstone_Employees_Email",
                table: "kumarcapstone_Employees");

            migrationBuilder.DropIndex(
                name: "IX_kumarcapstone_Departments_Name",
                table: "kumarcapstone_Departments");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "kumarcapstone_Employees");
        }
    }
}
