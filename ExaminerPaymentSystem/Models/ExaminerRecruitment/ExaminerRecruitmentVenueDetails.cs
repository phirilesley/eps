using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExaminerPaymentSystem.Models.ExaminerRecruitment
{
    public class ExaminerRecruitmentVenueDetails
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Venue Name")]
        public string VenueName { get; set; }

        [Required]
        [DisplayName("Trainning Start Date")]
        public DateTime TrainingStartDate { get; set; }

        [Required]
        [DisplayName("Training End Date")]
        public DateTime TrainingEndDate { get; set; }

        [Required]
        [DisplayName("Check In Date")]
        public DateTime CheckInDate { get; set; }


        [Required]
        [DisplayName("Check Out Date")]
        public DateTime? CheckOutDate { get; set; }

        [DisplayName("Training Time")]
        public TimeOnly? TrainingTime { get; set; }

        [ValidateNever]
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        // Navigation property for the relationship

        [ValidateNever]
        public ICollection<ExaminerRecruitmentInvitation> ExaminerRecruitmentInvitations { get; set; }
    }
}
