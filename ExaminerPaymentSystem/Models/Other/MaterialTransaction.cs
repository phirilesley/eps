using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.Other
{
    public class MaterialTransaction
    {
        [Key]
        public int Id { get; set; }


        public string SUBSUBID { get; set; }


        public string PAPERCODE { get; set; }

        public string? Region {  get; set; }


        public string? ITEM { get; set; }


        public int QUANTITY { get; set; }


    }
}
