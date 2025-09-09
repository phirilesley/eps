using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class addnewregister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SubKey",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ClusterManagerComment",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionalManagerComment",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamMonitorRegisterDates_SubKey",
                table: "ExamMonitorRegisterDates",
                column: "SubKey");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamMonitorRegisterDates_ExamMonitorTransactions_SubKey",
                table: "ExamMonitorRegisterDates",
                column: "SubKey",
                principalTable: "ExamMonitorTransactions",
                principalColumn: "SubKey",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamMonitorRegisterDates_ExamMonitorTransactions_SubKey",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropIndex(
                name: "IX_ExamMonitorRegisterDates_SubKey",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "ClusterManagerComment",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.DropColumn(
                name: "RegionalManagerComment",
                table: "ExamMonitorRegisterDates");

            migrationBuilder.AlterColumn<string>(
                name: "SubKey",
                table: "ExamMonitorRegisterDates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
