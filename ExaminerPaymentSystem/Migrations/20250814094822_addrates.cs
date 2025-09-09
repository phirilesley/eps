using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class addrates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Centres_Clusters_ClusterCode",
                table: "Centres");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_Centres_ClusterCode",
                table: "Centres");

            migrationBuilder.AddColumn<decimal>(
                name: "AccomodationRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BreakFastRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DinnerRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LunchRate",
                table: "Phases",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "ClusterCode",
                table: "Centres",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "IsCluster",
                table: "Centres",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccomodationRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "BreakFastRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "DinnerRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "LunchRate",
                table: "Phases");

            migrationBuilder.DropColumn(
                name: "IsCluster",
                table: "Centres");

            migrationBuilder.AlterColumn<string>(
                name: "ClusterCode",
                table: "Centres",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    ClusterCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CentreName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CentreNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClusterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.ClusterCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Centres_ClusterCode",
                table: "Centres",
                column: "ClusterCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Centres_Clusters_ClusterCode",
                table: "Centres",
                column: "ClusterCode",
                principalTable: "Clusters",
                principalColumn: "ClusterCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
