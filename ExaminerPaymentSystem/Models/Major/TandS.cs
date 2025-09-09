using ExaminerPaymentSystem.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.Major
{
    public class TandS
    {
        [Key]
        public string TANDSCODE { get; set; }

        [Required]
        public string EMS_EXAMINER_CODE { get; set; }

        [ForeignKey("Examiner")]
        public string EMS_NATIONAL_ID { get; set; }
        [Required]
        public string EMS_SUBKEY { get; set; }
        public string? DATE { get; set; }
        public string? EMS_PURPOSEOFJOURNEY { get; set; }
        public string? EMS_VENUE { get; set; }
        public decimal? EMS_TOTAL { get; set; }
        public decimal? ADJ_TOTAL { get; set; }
        public string? CENTRE_SUPERVISOR_STATUS { get; set; }
        public string? CENTRE_SUPERVISOR_STATUS_BY { get; set; }
        public string? CENTRE_SUPERVISOR_DATE { get; set; }
        public string? CENTRE_SUPERVISOR_COMMENT { get; set; }
        public string? SUBJECT_MANAGER_STATUS { get; set; }
        public string? SUBJECT_MANAGER_STATUS_BY { get; set; }
        public string? SUBJECT_MANAGER_DATE { get; set; }
        public string? SUBJECT_MANAGER_COMMENT { get; set; }
        public string? ACCOUNTS_STATUS { get; set; }
        public string? ACCOUNTS_STATUS_BY { get; set; }
        public string? ACCOUNTS_DATE { get; set; }
        public string? ACCOUNTS_REVIEW { get; set; }
        public string? ACCOUNTS_REVIEW_BY { get; set; }
        public string? ACCOUNTS_REVIEW_DATE { get; set; }
        public string? ACCOUNTS_REVIEW_COMMENT { get; set; }
        public string? STATUS { get; set; }
        public string? STATUS_BY { get; set; }
        public string? STATUS_DATE { get; set; }
        public string? STATUS_COMMENT { get; set; }
        public string? ADJ_BY { get; set; }
        public string? DATE_ADJ { get; set; }
        public string? ReturnBackStatus { get; set; }
        public string? ReturnBackBy { get; set; }
        public string? ReturnDate { get; set; }
        public string? ReturnComment { get; set; }

        public string? PaidStatus { get; set; }
        public string? PaidStatusBy { get; set; }
        public string? PaidStatusDate { get; set; }
        public string? PaidStatusComment { get; set; }

        public decimal? PaidAmount { get; set; }

        // Navigation properties
        public Examiner Examiner { get; set; }

        public TandSAdvance TandSAdvance { get; set; }

        public ICollection<TandSDetail> TandSDetails { get; set; } = new List<TandSDetail>();

        public ICollection<TandSFile> TandSFiles { get; set; } = new List<TandSFile>();
    }

    public class TandSDetail
    {
        [Key]
        public int Id { get; set; }
        public string? EMS_EXAMINER_CODE { get; set; }
        public string? EMS_SUBKEY { get; set; }

        [ForeignKey("TandS")]
        public string TANDSCODE { get; set; }

  
        public string? EMS_NATIONAL_ID { get; set; }
        public string? EMS_DATE { get; set; }
        public string? EMS_DEPARTURE { get; set; }
        public string? EMS_ARRIVAL { get; set; }
        public string? EMS_PLACE { get; set; }
        public decimal? EMS_BUSFARE { get; set; }
        public decimal? EMS_ACCOMMODATION { get; set; }
        public decimal? EMS_LUNCH { get; set; }
        public decimal? EMS_DINNER { get; set; }
        public decimal? EMS_TOTAL { get; set; }
        public decimal? ADJ_BUSFARE { get; set; }
        public decimal? ADJ_ACCOMMODATION { get; set; }
        public decimal? ADJ_LUNCH { get; set; }
        public decimal? ADJ_DINNER { get; set; }
        public decimal? ADJ_TOTAL { get; set; }
        public string? ADJ_BY { get; set; }
        public string? ADJ_DATE { get; set; }
        // Foreign key
        
        public TandS TandS { get; set; }
    }

    public class TandSAdvance
    {
        [Key]
        public int Id { get; set; }
        public string? EMS_EXAMINER_CODE { get; set; }
        public string? EMS_SUBKEY { get; set; }

        [ForeignKey("TandS")]
        public string TANDSCODE { get; set; }


        public string? EMS_NATIONAL_ID { get; set; }
        public string? EMS_DATE { get; set; }
        public string? ADV_STATUS { get; set; }
        public decimal? ADV_TEAS { get; set; }
        public decimal? ADV_BREAKFAST { get; set; }
        public decimal? ADV_TRANSPORT { get; set; }
        public decimal? ADV_ACCOMMODATION_RES { get; set; }
        public decimal? ADV_ACCOMMODATION_NONRES { get; set; }
        public decimal? ADV_LUNCH { get; set; }
        public decimal? ADV_DINNER { get; set; }
        public decimal? ADV_OVERNIGHTALLOWANCE { get; set; }
        public decimal? ADV_TOTAL { get; set; }
        public decimal? ADJ_ADV_TEAS { get; set; }
        public decimal? ADJ_ADV_BREAKFAST { get; set; }
        public decimal? ADJ_ADV_TRANSPORT { get; set; }
        public decimal? ADJ_ADV_ACCOMMODATION_RES { get; set; }
        public decimal? ADJ_ADV_ACCOMMODATION_NONRES { get; set; }
        public decimal? ADJ_ADV_LUNCH { get; set; }
        public decimal? ADJ_ADV_DINNER { get; set; }
        public decimal? ADJ_ADV_OVERNIGHTALLOWANCE { get; set; }
        public decimal? ADJ_ADV_TOTAL { get; set; }



        public TandS TandS { get; set; }
    }

    public class TandSAdvanceFees
    {
        [Key]
        public int Id { get; set; }

        public decimal? FEE_TEA { get; set; }
        public decimal? FEE_BREAKFAST { get; set; }
        public decimal? FEE_TRANSPORT { get; set; }
        public decimal? FEE_ACCOMMODATION_RES { get; set; }
        public decimal? FEE_ACCOMMODATION_NONRES { get; set; }
        public decimal? FEE_LUNCH { get; set; }

        public decimal? FEE_DINNER { get; set; }

        public decimal? FEE_OVERNIGHTALLOWANCE { get; set; }


    }

    public class TandSFile
    {
        [Key]
        public int Id { get; set; }
        public string? EMS_EXAMINER_CODE { get; set; }
        public string? EMS_SUBKEY { get; set; }

        [ForeignKey("TandS")]
        public string TANDSCODE { get; set; }

  
        public string? EMS_NATIONAL_ID { get; set; }
        public string FileName { get; set; }
        public string Currency { get; set; }

        // Foreign key

        public TandS TandS { get; set; }
    }

    public class FileDocument
    {
        public string FileCurrency { get; set; }

        public IEnumerable<IFormFile> Files { get; set; }
    }

}
