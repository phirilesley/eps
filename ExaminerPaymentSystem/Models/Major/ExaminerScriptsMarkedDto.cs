using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Major
{
    public class ExaminerScriptsMarkedDto
    {
        [Key]
        public string? EMS_EXAMINER_NUMBER { get; set; }
        public string? EMS_EXAMINER_NAME { get; set; }

        public string? EMS_SUBJECT_CODE { get; set; }
        public string? EMS_PAPER_CODE { get; set; }

        public int? SCRIPTS_MARKED { get; set; }

        public string? EMS_NATIONAL_ID { get; set; }

        public string? EMS_ECT_EXAMINER_CAT_CODE { get; set; }

        public string? SCRIPT_RATE { get; set; }

        public string? MODERATION_FEES { get; set; }

        public string? RESPONSIBILITY_FEES { get; set; }

        public string? COORDINATION_FEES { get; set; }

        public string? CAPTURING_FEES { get; set; }

        public string? GRAND_TOTAL { get; set; }

        public string? EMS_EXAM_CODE { get; set; }

        public string? EMS_EXAMINERCODE { get; set; }
        public string? EMS_SUBKEY { get; set; }

        public string? STATUS { get; set; }

        public string? EMS_MARKING_REG_CODE { get; set; }
        public string? EMS_EXM_SUPERORD { get; set; }
        public string? EMS_ACTIVITY { get; set; }


        public bool IsPresent { get; set; }

        public string? RegisterStatus { get; set; }
        public string? RegisterStatusBy { get; set; }
        public string? RegisterStatusDate { get; set; }
        public string? RecommendedBy { get; set; }
        public string? RecommendedDate { get; set; }
        public string? RecommendedStatus { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? AttendanceStatusBy { get; set; }
        public string? AttendanceStatusDate { get; set; }
    }
}
