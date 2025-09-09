using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Data
{
    public class ApplicationUserDTO 
    {
        [Key]
        public int Id { get; set; }
        public string FirstName {  get; set; }

        public string Surname { get; set; }
        public string? RegionCode { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string IDNumber { get; set; }
        public bool Activated { get; set; }
    }
}
