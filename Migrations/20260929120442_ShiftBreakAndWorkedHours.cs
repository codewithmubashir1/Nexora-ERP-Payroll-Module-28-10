using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Identity.Migrations
{
    /// <inheritdoc />
    public partial class ShiftBreakAndWorkedHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakMinutes",
                table: "ShiftTimings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkedHours",
                table: "AttendanceRecords",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE ShiftTimings SET StartTime = '08:00:00', BreakMinutes = 60 WHERE Name = '08 Hours' AND StartTime = '09:00:00' AND EndTime = '17:00:00' AND BreakMinutes = 0");
            migrationBuilder.Sql("UPDATE ShiftTimings SET StartTime = '08:00:00', BreakMinutes = 60 WHERE Name = '10 Hours' AND StartTime = '09:00:00' AND EndTime = '19:00:00' AND BreakMinutes = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE ShiftTimings SET StartTime = '09:00:00', BreakMinutes = 0 WHERE Name = '08 Hours' AND StartTime = '08:00:00' AND EndTime = '17:00:00' AND BreakMinutes = 60");
            migrationBuilder.Sql("UPDATE ShiftTimings SET StartTime = '09:00:00', BreakMinutes = 0 WHERE Name = '10 Hours' AND StartTime = '08:00:00' AND EndTime = '19:00:00' AND BreakMinutes = 60");
            migrationBuilder.DropColumn(
                name: "BreakMinutes",
                table: "ShiftTimings");

            migrationBuilder.DropColumn(
                name: "WorkedHours",
                table: "AttendanceRecords");
        }
    }
}
