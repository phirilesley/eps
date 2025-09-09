using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class EntriesTranscribersData
    {
        [Key]
        public int Id { get; set; } // Primary Key, auto-incremented

        [MaxLength(50)]
        public string? AppointedScripts { get; set; } // Nullable string with max length 
    }
}
