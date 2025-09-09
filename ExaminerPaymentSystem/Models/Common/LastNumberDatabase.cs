using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Common
{
    public class LastNumberDatabase
    {
        [Key]
        public int Id { get; set; }

        public int MaxNumber { get; set; }
    }
}
