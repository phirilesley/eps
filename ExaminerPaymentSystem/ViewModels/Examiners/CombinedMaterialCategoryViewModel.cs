namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class CombinedMaterialCategoryViewModel
    {
        public string ExamCode { get; set; }
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }

        public string? Region { get; set; }
        public List<MaterialTransactionViewModel> Materials { get; set; }
        public List<CategoryDateViewModel> CategoryDates { get; set; }
    }
}
