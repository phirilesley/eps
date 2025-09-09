using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class newchangestorecruitment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsRegisters",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberUsd",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberZwg",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchCodeUsd",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchCodeZwg",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCodeUsd",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCodeZwg",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankNameUsd",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankNameZwg",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchUsd",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchZwg",
                table: "ExamMonitorsRecruitments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsClaimTandSs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorTransactions_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorTransactions",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorsRegisters_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsRegisters",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorsClaimTandSs_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsClaimTandSs",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ExamMonitorsRecruitmentNationalId",
                table: "AspNetUsers",
                column: "ExamMonitorsRecruitmentNationalId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "AspNetUsers",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorsClaimTandSs_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsClaimTandSs",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorsRegisters_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsRegisters",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorTransactions_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorTransactions",
                column: "ExamMonitorsRecruitmentNationalId",
                principalTable: "ExamMonitorsRecruitments",
                principalColumn: "NationalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorsClaimTandSs_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorsRegisters_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsRegisters");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorTransactions_ExamMonitorsRecruitments_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorTransactions_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorsRegisters_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsRegisters");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorsClaimTandSs_ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ExamMonitorsRecruitmentNationalId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorTransactions");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsRegisters");

            migrationBuilder.DropColumn(
                name: "AccountNumberUsd",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "AccountNumberZwg",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BankBranchCodeUsd",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BankBranchCodeZwg",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BankCodeUsd",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BankCodeZwg",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BankNameUsd",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BankNameZwg",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BranchUsd",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "BranchZwg",
                table: "ExamMonitorsRecruitments");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "ExamMonitorsClaimTandSs");

            migrationBuilder.DropColumn(
                name: "ExamMonitorsRecruitmentNationalId",
                table: "AspNetUsers");
        }
    }
}
