using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors.Dtos
{
    // ExamMonitorDTO.cs
    public class ExamMonitorDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public string? Sex { get; set; }
        public string? Status { get; set; }
        public string? Qualification { get; set; }
        public string? Experience { get; set; }
        public string Region { get; set; }
        public string? Phone { get; set; }
        public string? Age { get; set; }
        public string Centre { get; set; }
        public string? Station { get; set; }
        public string? District { get; set; }
    }

   
}
