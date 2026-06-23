using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailFrameworkFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTimesheetFrozen",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReminderCount",
                table: "Timesheets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "HoursWorked",
                table: "TimesheetEntries",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTimesheetFrozen",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReminderCount",
                table: "Timesheets");

            migrationBuilder.AlterColumn<decimal>(
                name: "HoursWorked",
                table: "TimesheetEntries",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);
        }
    }
}
