using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class fixnewtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Region",
                table: "Clusters",
                newName: "RegionCode");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Clusters",
                newName: "DistrictName");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "Centres",
                newName: "RegionCode");

            migrationBuilder.RenameColumn(
                name: "IsResidentMonitor",
                table: "Centres",
                newName: "IsResident");

            migrationBuilder.RenameColumn(
                name: "IsGrade7",
                table: "Centres",
                newName: "DistrictCode");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Centres",
                newName: "ClusterName");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "ExamMonitorTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DistrictCode",
                table: "Clusters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Centres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Centres_DistrictId",
                table: "Centres",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Centres_Districts_DistrictId",
                table: "Centres",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Centres_Districts_DistrictId",
                table: "Centres");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropIndex(
                name: "IX_Centres_DistrictId",
                table: "Centres");

            migrationBuilder.DropColumn(
                name: "District",
                table: "ExamMonitorTransactions");

            migrationBuilder.DropColumn(
                name: "DistrictCode",
                table: "Clusters");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Centres");

            migrationBuilder.RenameColumn(
                name: "RegionCode",
                table: "Clusters",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "DistrictName",
                table: "Clusters",
                newName: "District");

            migrationBuilder.RenameColumn(
                name: "RegionCode",
                table: "Centres",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "IsResident",
                table: "Centres",
                newName: "IsResidentMonitor");

            migrationBuilder.RenameColumn(
                name: "DistrictCode",
                table: "Centres",
                newName: "IsGrade7");

            migrationBuilder.RenameColumn(
                name: "ClusterName",
                table: "Centres",
                newName: "District");
        }
    }
}
