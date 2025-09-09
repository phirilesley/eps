using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class hhhhhhh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // 1. First create the table if it doesn't exist
            migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamMonitor')
        BEGIN
            CREATE TABLE [ExamMonitor] (
                [NationalId] nvarchar(450) NOT NULL,
                [MonitorId] uniqueidentifier NOT NULL DEFAULT NEWID(),
                [FirstName] nvarchar(60) NOT NULL,
                [LastName] nvarchar(60) NOT NULL,
                [Sex] nvarchar(1) NULL,
                [Status] nvarchar(60) NULL,
                [Qualification] nvarchar(30) NULL,
                [Experience] nvarchar(60) NULL,
                [Region] nvarchar(2) NOT NULL,
                [Phone] nvarchar(10) NULL,
                [Age] nvarchar(3) NULL,
                [Centre] nvarchar(6) NOT NULL,
                [Station] nvarchar(60) NULL,
                [District] nvarchar(60) NULL,
                [BankNameZwg] nvarchar(max) NULL,
                [BranchZwg] nvarchar(max) NULL,
                [AccountNumberZwg] nvarchar(max) NULL,
                [BankNameUsd] nvarchar(max) NULL,
                [BranchUsd] nvarchar(max) NULL,
                [AccountNumberUsd] nvarchar(max) NULL,
                CONSTRAINT [PK_ExamMonitor] PRIMARY KEY ([NationalId])
            );
        END
    ");

            // 2. Now safely drop constraints/columns
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamMonitor')
        BEGIN
            IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'AK_ExamMonitor_TempId')
            BEGIN
                ALTER TABLE [ExamMonitor] DROP CONSTRAINT [AK_ExamMonitor_TempId];
            END
            
            IF EXISTS (SELECT * FROM sys.columns WHERE name = 'TempId' AND object_id = OBJECT_ID('ExamMonitor'))
            BEGIN
                ALTER TABLE [ExamMonitor] DROP COLUMN [TempId];
            END
        END
    ");

            // 3. Handle AspNetUsers changes
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
        BEGIN
            IF EXISTS (SELECT * FROM sys.columns WHERE name = 'ExamMonitorNationalId' AND object_id = OBJECT_ID('AspNetUsers'))
            BEGIN
                ALTER TABLE [AspNetUsers] DROP COLUMN [ExamMonitorNationalId];
            END
        END
    ");



            migrationBuilder.RenameTable(
                name: "ExamMonitor",
                newName: "ExamMonitors");

      

           

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    ClusterCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClusterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.ClusterCode);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorTransactions",
                columns: table => new
                {
                    SubKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MonitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CentreAttached = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorTransactions", x => x.SubKey);
                    table.ForeignKey(
                        name: "FK_ExamMonitorTransactions_ExamMonitors_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ExamMonitors",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionYear = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    LevelCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.LevelCode);
                });

            migrationBuilder.CreateTable(
                name: "Centres",
                columns: table => new
                {
                    CentreNumber = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    CentreName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SchoolType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centres", x => x.CentreNumber);
                    table.ForeignKey(
                        name: "FK_Centres_Clusters_ClusterCode",
                        column: x => x.ClusterCode,
                        principalTable: "Clusters",
                        principalColumn: "ClusterCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorsClaimTandSs",
                columns: table => new
                {
                    ClaimID = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    SubKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CentreAttached = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    ClusterManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterManagerStatusBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterManagerStatusDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegionalManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalManagerStatusBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalManagerStatusDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidStatusBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidStatusDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidStatusComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorsClaimTandSs", x => new { x.SubKey, x.ClaimID });
                    table.ForeignKey(
                        name: "FK_ExamMonitorsClaimTandSs_ExamMonitorTransactions_SubKey",
                        column: x => x.SubKey,
                        principalTable: "ExamMonitorTransactions",
                        principalColumn: "SubKey",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ExamMonitorsClaimTandSs_ExamMonitors_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ExamMonitors",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorsRegisters",
                columns: table => new
                {
                    SubKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompiledStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompiledBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompiledDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterManagerBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterManagerDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalManagerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalManagerBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionalManagerDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    IsPresentBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPresentDate = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorsRegisters", x => new { x.SubKey, x.Date });
                    table.ForeignKey(
                        name: "FK_ExamMonitorsRegisters_ExamMonitorTransactions_SubKey",
                        column: x => x.SubKey,
                        principalTable: "ExamMonitorTransactions",
                        principalColumn: "SubKey",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ExamMonitorsRegisters_ExamMonitors_NationalId",
                        column: x => x.NationalId,
                        principalTable: "ExamMonitors",
                        principalColumn: "NationalId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Phases",
                columns: table => new
                {
                    PhaseCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    PhaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhaseYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LevelCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phases", x => x.PhaseCode);
                    table.ForeignKey(
                        name: "FK_Phases_Levels_LevelCode",
                        column: x => x.LevelCode,
                        principalTable: "Levels",
                        principalColumn: "LevelCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorsDailyAdvances",
                columns: table => new
                {
                    SubKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lunch = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Breakfast = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Accomodation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Dinner = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorsDailyAdvances", x => new { x.SubKey, x.ClaimID });
                    table.ForeignKey(
                        name: "FK_ExamMonitorsDailyAdvances_ExamMonitorsClaimTandSs_SubKey_ClaimID",
                        columns: x => new { x.SubKey, x.ClaimID },
                        principalTable: "ExamMonitorsClaimTandSs",
                        principalColumns: new[] { "SubKey", "ClaimID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamMonitorsStipendAdvances",
                columns: table => new
                {
                    SubKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhaseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamMonitorsStipendAdvances", x => new { x.SubKey, x.ClaimID });
                    table.ForeignKey(
                        name: "FK_ExamMonitorsStipendAdvances_ExamMonitorsClaimTandSs_SubKey_ClaimID",
                        columns: x => new { x.SubKey, x.ClaimID },
                        principalTable: "ExamMonitorsClaimTandSs",
                        principalColumns: new[] { "SubKey", "ClaimID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Centres_ClusterCode",
                table: "Centres",
                column: "ClusterCode");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorsClaimTandSs_NationalId",
                table: "ExamMonitorsClaimTandSs",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorsClaimTandSs_SubKey",
                table: "ExamMonitorsClaimTandSs",
                column: "SubKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorsRegisters_NationalId",
                table: "ExamMonitorsRegisters",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorTransactions_NationalId",
                table: "ExamMonitorTransactions",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Phases_LevelCode",
                table: "Phases",
                column: "LevelCode");

          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ExamMonitors_IDNumber",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Centres");

            migrationBuilder.DropTable(
                name: "ExamMonitorsDailyAdvances");

            migrationBuilder.DropTable(
                name: "ExamMonitorsRegisters");

            migrationBuilder.DropTable(
                name: "ExamMonitorsStipendAdvances");

            migrationBuilder.DropTable(
                name: "ExamSessions");

            migrationBuilder.DropTable(
                name: "Phases");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropTable(
                name: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DropTable(
                name: "ExamMonitorTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamMonitors",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "AccountNumberUsd",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "AccountNumberZwg",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "BankNameUsd",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "BankNameZwg",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "BranchUsd",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "BranchZwg",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Centre",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "District",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "MonitorId",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Station",
                table: "ExamMonitors");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExamMonitors");

            migrationBuilder.RenameTable(
                name: "ExamMonitors",
                newName: "ExamMonitor");

            migrationBuilder.AddColumn<int>(
                name: "ExamMonitorNationalId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempId",
                table: "ExamMonitor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ExamMonitor_TempId",
                table: "ExamMonitor",
                column: "TempId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ExamMonitor_ExamMonitorNationalId",
                table: "AspNetUsers",
                column: "ExamMonitorNationalId",
                principalTable: "ExamMonitor",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
