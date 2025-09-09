using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors.Dtos
{
    public class ExamMonitorCreateDTO
    {
        [Required]
        [StringLength(60)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(60)]
        public string LastName { get; set; }

        [Required]
        [StringLength(60)]
        public string NationalId { get; set; }

        [StringLength(1)]
        public string? Sex { get; set; }

        [StringLength(60)]
        public string? Status { get; set; }

        [StringLength(30)]
        public string? Qualification { get; set; }

        [StringLength(60)]
        public string? Experience { get; set; }

        [Required]
        [StringLength(2)]
        public string Region { get; set; }

        [StringLength(10)]
        public string? Phone { get; set; }

        [StringLength(3)]
        public string? Age { get; set; }

        [Required]
        [StringLength(6)]
        public string Centre { get; set; }

        [StringLength(60)]
        public string? Station { get; set; }

        [StringLength(60)]
        public string? District { get; set; }
    }
}
