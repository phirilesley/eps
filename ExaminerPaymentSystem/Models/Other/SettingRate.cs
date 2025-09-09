using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class SettingRate
    {
        [Key]
        public int Id { get; set; }
        public string? SubSubId { get; set; }
        public string? PaperCode { get; set; }
        public decimal? SettingFee { get; set; } // nullable
    }
}
