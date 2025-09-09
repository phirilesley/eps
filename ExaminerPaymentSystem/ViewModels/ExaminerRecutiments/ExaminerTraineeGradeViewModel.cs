namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class ExaminerTraineeGradeViewModel
    {
 
            public int ExaminerRecruitmentId { get; set; }
            public string? Grade { get; set; } // Changed from char to string
            public string? Status { get; set; }
            public string? Comments { get; set; }
            public int? ExaminerRecruitmentRegisterId { get; set; }
            public DateTime Date { get; set; }
        


    }
}
