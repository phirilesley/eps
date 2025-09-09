using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class startdateandenddate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AssistantClusterManagerRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ClusterManagerRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ResidentMonitorRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AjustedDays",
                table: "ExamMonitorsClaimTandSs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ExamMonitorsClaimTandSs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ExamMonitorsClaimTandSs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssistantClusterManagerRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "ClusterManagerRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "ResidentMonitorRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "AjustedDays",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ExamMonitorsClaimTandSs");
        }
    }
}
