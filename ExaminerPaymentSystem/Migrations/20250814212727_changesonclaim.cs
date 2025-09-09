using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class changesonclaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InitiatorStatus",
                table: "ExamMonitorsClaimTandSs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InitiatorStatusBy",
                table: "ExamMonitorsClaimTandSs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "InitiatorStatusDate",
                table: "ExamMonitorsClaimTandSs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "ExamMonitorsClaimTandSs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatusBy",
                table: "ExamMonitorsClaimTandSs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewStatusDate",
                table: "ExamMonitorsClaimTandSs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatorStatus",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "InitiatorStatusBy",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "InitiatorStatusDate",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "ReviewStatusBy",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "ReviewStatusDate",
                table: "ExamMonitorsClaimTandSs");
        }
    }
}
