using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class addDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFromTimetable",
                table: "ExamMonitorRegisterDates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSupervisorAdded",
                table: "ExamMonitorRegisterDates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorBy",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorComment",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupervisorDate",
                table: "ExamMonitorRegisterDates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorStatus",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFromTimetable",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "IsSupervisorAdded",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "SupervisorBy",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "SupervisorComment",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "SupervisorDate",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "SupervisorStatus",
                table: "ExamMonitorRegisterDates");
        }
    }
}
