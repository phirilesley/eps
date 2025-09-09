using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Models.Other
{
    public class AdvanceComment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("TandS")]
        public string TANDSCODE { get; set; }
        public string? ExaminerCode { get; set; }
        public string? IdNumber { get; set; }
        public string? SubKey { get; set; }
        public string? Comment { get; set; }
        public decimal? Amount { get; set; } // Nullable decimal
        public string? DeletedOrRejectedBy { get; set; }

        public TandS TandS { get; set; }
    }
}
