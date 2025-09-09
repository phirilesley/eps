using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class ExaminerApportionment
    {
        [Key]
        public int Id { get; set; }
        public string? category { get; set; }
        public string? sub_sub_id { get; set; }
        public string? PaperCode { get; set; }

        public string? RegionCode { get; set; }

        public int TotalExaminers { get; set; }

        public int ScriptAToExaminerX { get; set; }

        public int ScriptPerExaminer {  get; set; }
    }
}
