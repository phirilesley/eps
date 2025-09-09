using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Common
{
    public class DateRange
    {
        [Key]
        public int Id { get; set; } // Unique identifier for each record

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } // Start date of the range

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
