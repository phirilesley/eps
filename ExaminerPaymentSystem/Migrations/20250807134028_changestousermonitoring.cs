using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class changestousermonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Phases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "AspNetUsers");
        }
    }
}
