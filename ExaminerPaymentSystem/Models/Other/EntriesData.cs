using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class EntriesData
    {
        [Key]
        public int Id { get; set; } // Primary Key, auto-incremented


        public string? ExamCode { get; set; } // Nullable string with max length of 50


        public string? Subject { get; set; } // Nullable string with max length of 100


        public string? PaperCode { get; set; } // Nullable string with max length of 50


        public string? BMS { get; set; } // Nullable string with max length of 50


        public string? AppointedScripts { get; set; } // Nullable string with max length 
    }
}
