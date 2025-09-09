using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminerPaymentSystem.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APPORTIONMENT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MKS_SUBJECT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SUB_SUBJECT_DESC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MKS_PAPER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PPR_PAPER_DESC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NUMBER_OF_CANDIDATES = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APPORTIONMENT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BANKING_DATA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    B_BANK_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    B_BANK_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BB_BRANCH_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BB_BRANCH_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BANKING_DATA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Braille_Transcription_Rate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade7 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ALevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Braille_Transcription_Rate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CAN_CATEGORY_RATE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ECT_EXAMINER_CAT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ECT_EXAMINER_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ECT_H_LEVEL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAN_CATEGORY_RATE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CAN_EXAM",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EXM_EXAM_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EXM_EXAM_SESSION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EXM_EXAM_YEAR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EXM_START_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EXM_EXAM_LEVEL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EXM_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EXM_CLOSE_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACTIVATED_SESSION = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAN_EXAM", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CAN_PAPER_MARKING_RATE",
                columns: table => new
                {
                    SUB_SUB_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PPR_PAPER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PPR_PAPER_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PPR_MARKING_RATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PPR_EXAM_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUB_SUBJECT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUB_SUBJECT_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAN_PAPER_MARKING_RATE", x => x.SUB_SUB_ID);
                });

            migrationBuilder.CreateTable(
                name: "DateRange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateRange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntriesData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaperCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BMS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointedScripts = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntriesData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EXM_CATEGORY_MARKING_RATE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PPR_EXAM_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ECT_EXAMINER_CAT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NAT_REP_ALLOWANCE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COORD_FEES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MOD_FEES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUPER_FEES = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXM_CATEGORY_MARKING_RATE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EXM_EXAMINER_MASTER",
                columns: table => new
                {
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_EXAMINER_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_LAST_NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_SEX = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_ADDRESS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_EXPERIENCE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_MARKING_EXPERIENCE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_LEVEL_OF_EXAM_MARKED = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_COMMENTS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PERFORMANCE_INDEX = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SELECTED_FLAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ECT_EXAMINER_CAT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SUB_SUB_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_WORK_ADD1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_WORK_ADD2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_WORK_ADD3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ADDRESS2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ADDRESS3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_WORK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_DISTRICT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_REGION_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PHONE_HOME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PHONE_BUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_QUALIFICATION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PAPER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_MARKING_REG_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_EXAMINER_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ACCOUNT_NO_FCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BANK_NAME_FCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BRANCH_NAME_FCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BANK_CODE_FCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BRANCH_CODE_FCA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ACCOUNT_NO_ZWL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BRANCH_NAME_ZWL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BANK_NAME_ZWL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BANK_CODE_ZWL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BRANCH_CODE_ZWL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_TAX_ID_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_EXM_SUPERORD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SEL_GRADING = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SEL_GRADE_REVIEW = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SEL_COORDINATION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_YEAR_TRAINED = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_DATE_OF_JOINING = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXM_EXAMINER_MASTER", x => x.EMS_NATIONAL_ID);
                });

            migrationBuilder.CreateTable(
                name: "EXM_SCRIPT_CAPTURED",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaperCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalScriptsCaptured = table.Column<int>(type: "int", nullable: false),
                    ScriptMarked = table.Column<int>(type: "int", nullable: false),
                    AccountsTotalScriptCaptured = table.Column<int>(type: "int", nullable: false),
                    AbsentScripts = table.Column<int>(type: "int", nullable: false),
                    ApportionedScripts = table.Column<int>(type: "int", nullable: false),
                    PirateCandidates = table.Column<int>(type: "int", nullable: false),
                    Exceptions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXM_SCRIPT_CAPTURED", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaxExaminerCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaxNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaxExaminerCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RATE_AND_TAX_INFO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RATE_AND_TAX_INFO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SUB_SUB_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SUB_SUBJECT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SUB_SUBJECT_DESC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SUB_SUB_ID);
                });

            migrationBuilder.CreateTable(
                name: "SubjectVenue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaperCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectVenue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TRAVELLING_AND_SUBSISTENCE_FEES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FEE_TEA = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEE_BREAKFAST = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEE_TRANSPORT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEE_ACCOMMODATION_RES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEE_ACCOMMODATION_NONRES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEE_LUNCH = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FEE_DINNER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FEE_OVERNIGHTALLOWANCE = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAVELLING_AND_SUBSISTENCE_FEES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VALIDATETANDS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExaminerCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfDays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaperName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompiledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompiledDate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VALIDATETANDS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VENUES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VENUES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZImSecStaff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZImSecStaff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ExaminerCode = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Activated = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_EXM_EXAMINER_MASTER_IDNumber",
                        column: x => x.IDNumber,
                        principalTable: "EXM_EXAMINER_MASTER",
                        principalColumn: "EMS_NATIONAL_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EXAMINER_TRANSACTIONS",
                columns: table => new
                {
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_SUB_SUB_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_PAPER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_EXAMINER_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_MARKING_REG_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ECT_EXAMINER_CAT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_EXM_SUPERORD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SCRIPTS_MARKED = table.Column<int>(type: "int", nullable: true),
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SCRIPT_RATE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MODERATION_FEES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RESPONSIBILITY_FEES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    COORDINATION_FEES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CAPTURING_FEES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GRAND_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EMS_COMPILED_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_COMPILED_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_COMPILED_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_APPROVED_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_APPROVED_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_APPROVED_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CERTIFIED_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CERTIFIED_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CERTIFIED_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CENTRE_SUPERVISOR_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CENTRE_SUPERVISOR_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CENTRE_SUPERVISOR_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_CAPTURINGROLE = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXAMINER_TRANSACTIONS", x => x.EMS_SUBKEY);
                    table.ForeignKey(
                        name: "FK_EXAMINER_TRANSACTIONS_EXM_EXAMINER_MASTER_EMS_NATIONAL_ID",
                        column: x => x.EMS_NATIONAL_ID,
                        principalTable: "EXM_EXAMINER_MASTER",
                        principalColumn: "EMS_NATIONAL_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerRegister",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminerCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendanceStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendanceStatusBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendanceUpdateDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusDate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerRegister", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminerRegister_EXM_EXAMINER_MASTER_IDNumber",
                        column: x => x.IDNumber,
                        principalTable: "EXM_EXAMINER_MASTER",
                        principalColumn: "EMS_NATIONAL_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminerSubject",
                columns: table => new
                {
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SUB_SUB_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PAPER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExaminerEMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminerSubject", x => x.EMS_NATIONAL_ID);
                    table.ForeignKey(
                        name: "FK_ExaminerSubject_EXM_EXAMINER_MASTER_ExaminerEMS_NATIONAL_ID",
                        column: x => x.ExaminerEMS_NATIONAL_ID,
                        principalTable: "EXM_EXAMINER_MASTER",
                        principalColumn: "EMS_NATIONAL_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                columns: table => new
                {
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PURPOSEOFJOURNEY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_VENUE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CENTRE_SUPERVISOR_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CENTRE_SUPERVISOR_STATUS_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CENTRE_SUPERVISOR_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CENTRE_SUPERVISOR_COMMENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUBJECT_MANAGER_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUBJECT_MANAGER_STATUS_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUBJECT_MANAGER_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUBJECT_MANAGER_COMMENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_STATUS_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_REVIEW = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_REVIEW_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_REVIEW_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCOUNTS_REVIEW_COMMENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS_COMMENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADJ_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATE_ADJ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnBackStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnBackBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAVELLING_AND_SUBSISTENCE_CLAIM", x => x.TANDSCODE);
                    table.ForeignKey(
                        name: "FK_TRAVELLING_AND_SUBSISTENCE_CLAIM_EXM_EXAMINER_MASTER_EMS_NATIONAL_ID",
                        column: x => x.EMS_NATIONAL_ID,
                        principalTable: "EXM_EXAMINER_MASTER",
                        principalColumn: "EMS_NATIONAL_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedTable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTrails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvanceComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExaminerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeletedOrRejectedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvanceComments_TRAVELLING_AND_SUBSISTENCE_CLAIM_TANDSCODE",
                        column: x => x.TANDSCODE,
                        principalTable: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                        principalColumn: "TANDSCODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeletedTandS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
               
                    ExaminerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeletedOrRejectedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedTandS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeletedTandS_TRAVELLING_AND_SUBSISTENCE_CLAIM_TANDSCODE1",
                        column: x => x.TANDSCODE,
                        principalTable: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                        principalColumn: "TANDSCODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnedTandS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
             
                    ExaminerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeletedOrRejectedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnedTandS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnedTandS_TRAVELLING_AND_SUBSISTENCE_CLAIM_TANDSCODE1",
                        column: x => x.TANDSCODE,
                        principalTable: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                        principalColumn: "TANDSCODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRAVELLING_AND_SUBSISTENCE_ADVANCES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADV_STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADV_TEAS = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_BREAKFAST = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_TRANSPORT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_ACCOMMODATION_RES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_ACCOMMODATION_NONRES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_LUNCH = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_DINNER = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_OVERNIGHTALLOWANCE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADV_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_TEAS = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_BREAKFAST = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_TRANSPORT = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_ACCOMMODATION_RES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_ACCOMMODATION_NONRES = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_LUNCH = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_DINNER = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_OVERNIGHTALLOWANCE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ADV_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAVELLING_AND_SUBSISTENCE_ADVANCES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRAVELLING_AND_SUBSISTENCE_ADVANCES_TRAVELLING_AND_SUBSISTENCE_CLAIM_TANDSCODE",
                        column: x => x.TANDSCODE,
                        principalTable: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                        principalColumn: "TANDSCODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRAVELLING_AND_SUBSISTENCE_DETAILS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_DEPARTURE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_ARRIVAL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_PLACE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_BUSFARE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EMS_ACCOMMODATION = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EMS_LUNCH = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EMS_DINNER = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EMS_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_BUSFARE = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_ACCOMMODATION = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_LUNCH = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_DINNER = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_TOTAL = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ADJ_BY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADJ_DATE = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAVELLING_AND_SUBSISTENCE_DETAILS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRAVELLING_AND_SUBSISTENCE_DETAILS_TRAVELLING_AND_SUBSISTENCE_CLAIM_TANDSCODE",
                        column: x => x.TANDSCODE,
                        principalTable: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                        principalColumn: "TANDSCODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRAVELLING_AND_SUBSISTENCE_FILES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EMS_EXAMINER_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMS_SUBKEY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TANDSCODE = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EMS_NATIONAL_ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRAVELLING_AND_SUBSISTENCE_FILES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRAVELLING_AND_SUBSISTENCE_FILES_TRAVELLING_AND_SUBSISTENCE_CLAIM_TANDSCODE",
                        column: x => x.TANDSCODE,
                        principalTable: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                        principalColumn: "TANDSCODE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceComments_TANDSCODE",
                table: "AdvanceComments",
                column: "TANDSCODE");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IDNumber",
                table: "AspNetUsers",
                column: "IDNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_UserId",
                table: "AuditTrails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedTandS_TANDSCODE1",
                table: "DeletedTandS",
                column: "TANDSCODE");

            migrationBuilder.CreateIndex(
                name: "IX_EXAMINER_TRANSACTIONS_EMS_NATIONAL_ID",
                table: "EXAMINER_TRANSACTIONS",
                column: "EMS_NATIONAL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EXAMINER_TRANSACTIONS_EMS_SUBKEY",
                table: "EXAMINER_TRANSACTIONS",
                column: "EMS_SUBKEY",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerRegister_IDNumber",
                table: "ExaminerRegister",
                column: "IDNumber",
                unique: true,
                filter: "[IDNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminerSubject_ExaminerEMS_NATIONAL_ID",
                table: "ExaminerSubject",
                column: "ExaminerEMS_NATIONAL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnedTandS_TANDSCODE1",
                table: "ReturnedTandS",
                column: "TANDSCODE");

            migrationBuilder.CreateIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_ADVANCES_TANDSCODE",
                table: "TRAVELLING_AND_SUBSISTENCE_ADVANCES",
                column: "TANDSCODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_CLAIM_EMS_NATIONAL_ID",
                table: "TRAVELLING_AND_SUBSISTENCE_CLAIM",
                column: "EMS_NATIONAL_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_DETAILS_TANDSCODE",
                table: "TRAVELLING_AND_SUBSISTENCE_DETAILS",
                column: "TANDSCODE");

            migrationBuilder.CreateIndex(
                name: "IX_TRAVELLING_AND_SUBSISTENCE_FILES_TANDSCODE",
                table: "TRAVELLING_AND_SUBSISTENCE_FILES",
                column: "TANDSCODE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvanceComments");

            migrationBuilder.DropTable(
                name: "APPORTIONMENT");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditTrails");

            migrationBuilder.DropTable(
                name: "BANKING_DATA");

            migrationBuilder.DropTable(
                name: "Braille_Transcription_Rate");

            migrationBuilder.DropTable(
                name: "CAN_CATEGORY_RATE");

            migrationBuilder.DropTable(
                name: "CAN_EXAM");

            migrationBuilder.DropTable(
                name: "CAN_PAPER_MARKING_RATE");

            migrationBuilder.DropTable(
                name: "DateRange");

            migrationBuilder.DropTable(
                name: "DeletedTandS");

            migrationBuilder.DropTable(
                name: "EntriesData");

            migrationBuilder.DropTable(
                name: "EXAMINER_TRANSACTIONS");

            migrationBuilder.DropTable(
                name: "ExaminerRegister");

            migrationBuilder.DropTable(
                name: "ExaminerSubject");

            migrationBuilder.DropTable(
                name: "EXM_CATEGORY_MARKING_RATE");

            migrationBuilder.DropTable(
                name: "EXM_SCRIPT_CAPTURED");

            migrationBuilder.DropTable(
                name: "MaxExaminerCode");

            migrationBuilder.DropTable(
                name: "RATE_AND_TAX_INFO");

            migrationBuilder.DropTable(
                name: "ReturnedTandS");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "SubjectVenue");

            migrationBuilder.DropTable(
                name: "TRAVELLING_AND_SUBSISTENCE_ADVANCES");

            migrationBuilder.DropTable(
                name: "TRAVELLING_AND_SUBSISTENCE_DETAILS");

            migrationBuilder.DropTable(
                name: "TRAVELLING_AND_SUBSISTENCE_FEES");

            migrationBuilder.DropTable(
                name: "TRAVELLING_AND_SUBSISTENCE_FILES");

            migrationBuilder.DropTable(
                name: "VALIDATETANDS");

            migrationBuilder.DropTable(
                name: "VENUES");

            migrationBuilder.DropTable(
                name: "ZImSecStaff");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TRAVELLING_AND_SUBSISTENCE_CLAIM");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "EXM_EXAMINER_MASTER");
        }
    }
}
