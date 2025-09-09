using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class addregisters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
  

            migrationBuilder.CreateTable(
                name: "ExamMonitorsRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompiledStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompiledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompiledDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterManagerBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterManagerDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionalManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionalManagerBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionalManagerDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    IsPresentBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPresentDate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorsRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorRegisterDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisterId = table.Column<int>(type: "int", nullable: false),
                    CompiledStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompiledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompiledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClusterManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterManagerBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterManagerDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegionalManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalManagerBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionalManagerDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorRegisterDates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamMonitorRegisterDates_ExamMonitorsRegisters_RegisterId",
                        column: x => x.RegisterId,
                        principalTable: "ExamMonitorsRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

       
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamMonitorRegisterDates");

            migrationBuilder.DropTable(
name: "ExamMonitorsRegisters");
        }
    }
}
