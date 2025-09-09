using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class attachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamMonitorAttachements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcademicQualifications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalIdDocs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorAttachements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamMonitorAttachements_ExamMonitors_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ExamMonitors",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorEmailInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvitedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateInvited = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorEmailInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamMonitorEmailInvitations_AspNetUsers_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamMonitorEmailInvitations_ExamMonitors_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ExamMonitors",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorProfessionalQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProgrammeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorProfessionalQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamMonitorProfessionalQualifications_ExamMonitors_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ExamMonitors",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorAttachements_NationalId",
                table: "ExamMonitorAttachements",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorEmailInvitations_InvitedBy",
                table: "ExamMonitorEmailInvitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorEmailInvitations_NationalId",
                table: "ExamMonitorEmailInvitations",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorProfessionalQualifications_NationalId",
                table: "ExamMonitorProfessionalQualifications",
                column: "NationalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamMonitorAttachements");

            migrationBuilder.DropTable(
                name: "ExamMonitorEmailInvitations");

            migrationBuilder.DropTable(
                name: "ExamMonitorProfessionalQualifications");
        }
    }
}
