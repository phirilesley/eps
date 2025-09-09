using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class newfieldsmonitor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedStatus",
                table: "ExamMonitorTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AcceptBy",
                table: "ExamMonitors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptDate",
                table: "ExamMonitors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AcceptStatus",
                table: "ExamMonitors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "ExamMonitors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorProfessionalQualifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorEmailInvitations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorAttachements",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamMonitorsRecruitments",
                columns: table => new
                {
                    NationalId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MonitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Experience = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Age = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Centre = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Station = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    District = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceptStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceptBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcceptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorsRecruitments", x => x.NationalId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorProfessionalQualifications_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorProfessionalQualifications",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorEmailInvitations_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorEmailInvitations",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorAttachements_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorAttachements",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorAttachements_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorAttachements",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorEmailInvitations_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorEmailInvitations",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorProfessionalQualifications_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorProfessionalQualifications",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorAttachements_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorAttachements");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorEmailInvitations_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorEmailInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorProfessionalQualifications_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorProfessionalQualifications");

            migrationBuilder.DropTable(
                name: "ExamMonitorsRecruitments");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorProfessionalQualifications_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorProfessionalQualifications");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorEmailInvitations_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorEmailInvitations");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorAttachements_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorAttachements");

            migrationBuilder.DropColumn(
                name: "AssignedStatus",
                table: "ExamMonitorTransactions");

            migrationBuilder.DropColumn(
                name: "AcceptBy",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "AcceptDate",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "AcceptStatus",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorProfessionalQualifications");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorEmailInvitations");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorAttachements");
        }
    }
}
