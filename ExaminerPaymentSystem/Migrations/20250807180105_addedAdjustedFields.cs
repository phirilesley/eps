using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class addedAdjustedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdjustedDays",
                table: "ExamMonitorsStipendAdvances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAdjustedAmount",
                table: "ExamMonitorsStipendAdvances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "ExamMonitorsStipendAdvances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedAccomodation",
                table: "ExamMonitorsDailyAdvances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedBreakfast",
                table: "ExamMonitorsDailyAdvances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedDinner",
                table: "ExamMonitorsDailyAdvances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdjustedLunch",
                table: "ExamMonitorsDailyAdvances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAdjustedAmount",
                table: "ExamMonitorsDailyAdvances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "ExamMonitorsDailyAdvances",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAdjustedAmount",
                table: "ExamMonitorsClaimTandSs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "ExamMonitorsClaimTandSs",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustedDays",
                table: "ExamMonitorsStipendAdvances");

            migrationBuilder.DropColumn(
                name: "TotalAdjustedAmount",
                table: "ExamMonitorsStipendAdvances");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "ExamMonitorsStipendAdvances");

            migrationBuilder.DropColumn(
                name: "AdjustedAccomodation",
                table: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropColumn(
                name: "AdjustedBreakfast",
                table: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropColumn(
                name: "AdjustedDinner",
                table: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropColumn(
                name: "AdjustedLunch",
                table: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropColumn(
                name: "TotalAdjustedAmount",
                table: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropColumn(
                name: "TotalAdjustedAmount",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "ExamMonitorsClaimTandSs");
        }
    }
}
