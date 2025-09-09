using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Status { get; set; }

        public string? Region { get; set; }
    }
}
