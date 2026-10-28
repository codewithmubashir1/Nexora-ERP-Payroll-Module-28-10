using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodGroupToEmployeesAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Employees",
                type: "TEXT",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "AspNetUsers");
        }
    }
}
