using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class ActivityRate
    {
        [Key]
        public int Id { get; set; }
        public string? Activity { get; set; }
        public decimal PercentageOfSetting { get; set; }
    }
}
