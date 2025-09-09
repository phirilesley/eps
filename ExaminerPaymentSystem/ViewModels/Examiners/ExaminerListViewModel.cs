using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class ExaminerListViewModel
    {
        public List<RegisterViewModel> Examiners { get; set; }
        public Pager Pager { get; set; }
        public string SearchTerm { get; set; }
        public string ExamCode { get; set; }
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }
    }
}
