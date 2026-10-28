using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Identity.Migrations
{
    /// <inheritdoc />
    public partial class PayRunDisbursementOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PayImmediately",
                table: "PayRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentAccountId",
                table: "PayRuns",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayImmediately",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "PaymentAccountId",
                table: "PayRuns");
        }
    }
}
