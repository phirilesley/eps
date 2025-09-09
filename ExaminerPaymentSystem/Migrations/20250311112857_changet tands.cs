using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class changettands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_CLAIM_EMS_NATIONAL_ID",
                table: "TRAVELLING_AND_SUBSISTENCE_CLAIM");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.AlterColumn<decimal>(
                name: "FEE_OVERNIGHTALLOWANCE",
                table: "TRAVELLING_AND_SUBSISTENCE_FEES",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FEE_DINNER",
                table: "TRAVELLING_AND_SUBSISTENCE_FEES",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceStatus",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceStatusBy",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceStatusDate",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EMS_ACTIVITY",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPresent",
                table: "EXAMINER_TRANSACTIONS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedBy",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedDate",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedStatus",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterStatus",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterStatusBy",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterStatusDate",
                table: "EXAMINER_TRANSACTIONS",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Activity",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_CLAIM_EMS_NATIONAL_ID",
                table: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                column: "EMS_NATIONAL_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_CLAIM_EMS_NATIONAL_ID",
                table: "TRAVELLING_AND_SUBSISTENCE_CLAIM");

            migrationBuilder.DropColumn(
                name: "AttendanceStatus",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "AttendanceStatusBy",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "AttendanceStatusDate",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "EMS_ACTIVITY",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "IsPresent",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "RecommendedBy",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "RecommendedDate",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "RecommendedStatus",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "RegisterStatus",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "RegisterStatusBy",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "RegisterStatusDate",
                table: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropColumn(
                name: "Activity",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "FEE_OVERNIGHTALLOWANCE",
                table: "TRAVELLING_AND_SUBSISTENCE_FEES",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FEE_DINNER",
                table: "TRAVELLING_AND_SUBSISTENCE_FEES",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "EXAMINER_TRANSACTIONS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_CLAIM_EMS_NATIONAL_ID",
                table: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                column: "EMS_NATIONAL_ID",
                unique: true);
        }
    }
}
