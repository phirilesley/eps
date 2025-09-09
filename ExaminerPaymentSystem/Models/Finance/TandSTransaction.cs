using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Finance
{
    public class TandSTransaction
    {
        [Key]
        public int Id { get; set; }
        public string NationalId { get; set; }  // Link to examiner/user
        public string EMS_SUBKEY { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string PaymentMethod { get; set; }  // Bank transfer, Mobile money, etc.
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Approved,
        Processed,
        Rejected,
        Paid,
        Failed
    }
}
