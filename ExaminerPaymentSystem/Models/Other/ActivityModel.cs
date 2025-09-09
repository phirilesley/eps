using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ExaminerPaymentSystem.Models.Other
{
    public class ActivityModel
    {
        [Required(ErrorMessage = "Please Select An Activity.")]
        public string Activity { get; set; }

        public List<Activity> Activities { get; } = new List<Activity>
    {
        new Activity { Value = "BEM", Text = "BELT MARKING" },
        new Activity { Value = "CAP", Text = "CAPTURING" },
        new Activity{Value = "TRB" , Text = "TRANSCRIBE"},
        new Activity { Value = "GRA", Text = "GRADING" },
        new Activity { Value = "REM", Text = "REMARKING" },
        new Activity { Value = "TRA", Text = "TRAINING" },
        new Activity { Value = "COO", Text = "COORDINATION" },
        new Activity { Value = "MOD", Text = "MODERATION" },
        new Activity { Value = "ITW", Text = "ITEM WRITING" },
        
    };
    }


    public class Activity
    {
        [Key]
        [Required]
        public string? Value { get; set; }

        [Required]
        public string? Text { get; set; }

        [Required]
        public string? Status { get; set; }

    }
}


