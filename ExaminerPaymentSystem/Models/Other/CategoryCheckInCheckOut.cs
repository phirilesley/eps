using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class CategoryCheckInCheckOut
    {
        [Key]
        public int Id { get; set; }

    
        public string SubSubId { get; set; }

  
        public string PaperCode { get; set; }

        public string? REGION {  get; set; }

        public string? Category { get; set; }


        public string? CheckIn { get; set; }

        public string? CheckOut { get; set; }
    }
}
