using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNewExaminerTrainingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExaminerRecruitment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CEM_CAN_EXAMINER_CODE = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CEM_CAN_EXAMINER_NAME = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CEM_ID = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CEM_ADDRESS = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    CEM_SCHOOL = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_STAGE_OF_SELECTION = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_SUBJECT = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CEM_QUALIFICATION = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_DATE_OF_JOINING = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CEM_EXPERIENCE = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_SELECTED_FLAG = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    CEM_WORK_ADD1 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_WORK_ADD2 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_WORK_ADD3 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_LAST_NAME = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CEM_ADDRESS2 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_ADDRESS3 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_DISTRICT_CODE = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_REGION_CODE = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CEM_PHONE_HOME = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CEM_SEX = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CEM_PHONE_BUS = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CEM_PAPER_CODE = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CEM_MARKING_EXP = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CEM_ACADEMIC_QUAL = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_YEAR_TRAINED = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CEM_ACCOUNT_NO = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_BANK_CODE = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CEM_BRANCH_CODE = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CEM_TRAINING_CENTRE = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CEM_PERFORMANCE_INDEX = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CEM_TRAINING_SESSION = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CEM_EMAIL_ADDRESS = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_CAPTURE_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CEM_CAPTURED_BY = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_ACTIONED_BY = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CEM_ACTION_DATE = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentVenueDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrainingStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrainingEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrainingTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentVenueDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentAttachements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    InstitutionHeadDoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcademicQualifications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalIdDocs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentAttachements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentAttachements_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentEmailInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    InvitedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateInvited = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentEmailInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentEmailInvitations_AspNetUsers_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentEmailInvitations_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentProfessionalQualifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    ProgrammeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentProfessionalQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentProfessionalQualifications_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentRegisters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentRegisters_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentTrainingSelection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentTrainingSelection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentTrainingSelection_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeachingExperiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    LevelTaught = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: true),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingExperiences_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    ExaminerRecruitmentVenueDetailsId = table.Column<int>(type: "int", nullable: false),
                    Attendance = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentInvitations_ExaminerRecruitmentVenueDetails_ExaminerRecruitmentVenueDetailsId",
                        column: x => x.ExaminerRecruitmentVenueDetailsId,
                        principalTable: "ExaminerRecruitmentVenueDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentInvitations_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRecruitmentAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerRecruitmentId = table.Column<int>(type: "int", nullable: false),
                    ExaminerRecruitmentRegisterId = table.Column<int>(type: "int", nullable: true),
                    CapturerGrade = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    VerifierGrade = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CapturerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VerifierId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRecruitmentAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentAssessments_AspNetUsers_CapturerId",
                        column: x => x.CapturerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentAssessments_AspNetUsers_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentAssessments_ExaminerRecruitmentRegisters_ExaminerRecruitmentRegisterId",
                        column: x => x.ExaminerRecruitmentRegisterId,
                        principalTable: "ExaminerRecruitmentRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminerRecruitmentAssessments_ExaminerRecruitment_ExaminerRecruitmentId",
                        column: x => x.ExaminerRecruitmentId,
                        principalTable: "ExaminerRecruitment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentAssessments_CapturerId",
                table: "ExaminerRecruitmentAssessments",
                column: "CapturerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentAssessments_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentAssessments",
                column: "ExaminerRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentAssessments_ExaminerRecruitmentRegisterId",
                table: "ExaminerRecruitmentAssessments",
                column: "ExaminerRecruitmentRegisterId",
                unique: true,
                filter: "[ExaminerRecruitmentRegisterId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentAssessments_VerifierId",
                table: "ExaminerRecruitmentAssessments",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentAttachements_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentAttachements",
                column: "ExaminerRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentEmailInvitations_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentEmailInvitations",
                column: "ExaminerRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentEmailInvitations_InvitedBy",
                table: "ExaminerRecruitmentEmailInvitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentInvitations_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentInvitations",
                column: "ExaminerRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentInvitations_ExaminerRecruitmentVenueDetailsId",
                table: "ExaminerRecruitmentInvitations",
                column: "ExaminerRecruitmentVenueDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentProfessionalQualifications_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentProfessionalQualifications",
                column: "ExaminerRecruitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentRegisters_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentRegisters",
                column: "ExaminerRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRecruitmentTrainingSelection_ExaminerRecruitmentId",
                table: "ExaminerRecruitmentTrainingSelection",
                column: "ExaminerRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingExperiences_ExaminerRecruitmentId",
                table: "TeachingExperiences",
                column: "ExaminerRecruitmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentAssessments");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentAttachements");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentEmailInvitations");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentInvitations");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentProfessionalQualifications");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentTrainingSelection");

            migrationBuilder.DropTable(
                name: "TeachingExperiences");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentRegisters");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitmentVenueDetails");

            migrationBuilder.DropTable(
                name: "ExaminerRecruitment");
        }
    }
}
