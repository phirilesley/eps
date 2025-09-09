using ExaminerPaymentSystem.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.Major
{
    public class ExaminerScriptsMarked
    {

        [Key]
        public string EMS_SUBKEY { get; set; }

        [Required]
        public string EMS_EXAMINER_CODE { get; set; }
        [Required]
        public string EMS_SUB_SUB_ID { get; set; }
        [Required]
        public string EMS_PAPER_CODE { get; set; }

        public string EMS_EXAMINER_NUMBER { get; set; }

        public string? EMS_MARKING_REG_CODE { get; set; }

        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }
        public string? EMS_EXM_SUPERORD { get; set; }

        public int? SCRIPTS_MARKED { get; set; }


        [ForeignKey("Examiner")]
        public string EMS_NATIONAL_ID { get; set; }



        public decimal? SCRIPT_RATE { get; set; }

        public decimal? MODERATION_FEES { get; set; }

        public decimal? RESPONSIBILITY_FEES { get; set; }

        public decimal? COORDINATION_FEES { get; set; }

        public decimal? CAPTURING_FEES { get; set; }

        public decimal? GRAND_TOTAL { get; set; }

        public string? EMS_COMPILED_BY { get; set; }

        public string? EMS_COMPILED_STATUS { get; set; }

        public string? EMS_COMPILED_DATE { get; set; }

        public string? EMS_APPROVED_BY { get; set; }

        public string? EMS_APPROVED_STATUS { get; set; }

        public string? EMS_APPROVED_DATE { get; set; }

        public string? EMS_CERTIFIED_BY { get; set; }

        public string? EMS_CERTIFIED_STATUS { get; set; }

        public string? EMS_CERTIFIED_DATE { get; set; }

        public string? EMS_CENTRE_SUPERVISOR_NAME { get; set; }

        public string? EMS_CENTRE_SUPERVISOR_STATUS { get; set; }

        public string? EMS_CENTRE_SUPERVISOR_DATE { get; set; }
        public string? EMS_CAPTURINGROLE { get; set; }

        public bool IsPresent { get; set; }
        public string? IsPresentBy { get; set; }
        public string? IsPresentDate { get; set; }
        public string? EMS_ACTIVITY {  get; set; }

        public string? RegisterStatus { get; set; }
        public string? RegisterStatusBy { get; set; }
        public string? RegisterStatusDate { get; set; }
        public string? RecommendedBy { get; set; }
        public string? RecommendedDate { get; set; }
        public string? RecommendedStatus { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? AttendanceStatusBy { get; set; }
        public string? AttendanceStatusDate { get; set; }

        public string? EMS_VENUE { get; set; }
        public string? PaidStatus { get; set; }
        public string? PaidStatusBy { get; set; }
        public string? PaidStatusDate { get; set; }
        public string? PaidStatusComment { get; set; }

        public decimal? PaidAmount { get; set; }

        public string? EMS_PERFORMANCE_INDEX { get; set; }
        public Examiner Examiner { get; set; }
  


    }


    public class ExaminerScriptsMarkedReportModel
    {
        public string EMS_EXAMINER_CODE { get; set; }

        public string? EMS_SUBKEY { get; set; }
        public string? SubSubId { get; set; }
        public string? PaperCode { get; set; }

        public int? SCRIPTS_MARKED { get; set; }

        public string? EMS_NATIONAL_ID { get; set; }

        public string? EMS_MARKING_REG_CODE { get; set; }

        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }


        public decimal? SCRIPT_RATE { get; set; }

        public decimal? MODERATION_FEES { get; set; }

        public decimal? RESPONSIBILITY_FEES { get; set; }

        public decimal? COORDINATION_FEES { get; set; }

        public decimal? CAPTURING_FEES { get; set; }

        public decimal? GRAND_TOTAL { get; set; }

        public string? EMS_CAPTURINGROLE { get; set; }

        public bool IsPresent { get; set; }
        public string? IsPresentBy { get; set; }
        public string? IsPresentDate { get; set; }

        public string? RegisterStatus { get; set; }
        public string? RegisterStatusBy { get; set; }
        public string? RegisterStatusDate { get; set; }
        public string? RecommendedBy { get; set; }
        public string? RecommendedDate { get; set; }
        public string? RecommendedStatus { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? AttendanceStatusBy { get; set; }
        public string? AttendanceStatusDate { get; set; }

        public string? EMS_VENUE {  get; set; }
        public string? PaidStatus {  get; set; }
        public string? PaidStatusBy { get; set; }
            public string? PaidStatusDate { get; set; }
        public string? PaidStatusComment {  get; set; }

    }
}
