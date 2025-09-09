using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;

namespace ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface
{


        // Repository Interface
        public interface IExaminerRecruitmentTrainingSelectionRepository
        {
            Task<IEnumerable<ExaminerRecruitmentTrainingSelection>> GetAllAsync();
            Task<ExaminerRecruitmentTrainingSelection> GetByIdAsync(int id);
            Task AddAsync(ExaminerRecruitmentTrainingSelection entity);
            Task UpdateAsync(ExaminerRecruitmentTrainingSelection entity);

         Task<(IEnumerable<ExaminerRecruitmentTrainingSelectionViewModel>, int, int)> GetTrainingSelectionDataTableAsync(
                int start, int length, string searchValue, string sortColumn, string sortDirection,
                string subject = null, string experience = null, string paperCode = null, string region = null);

        Task<(List<ExaminerRecruitmentTrainingSelectionViewModel> Data, int TotalCount, int TotalExaminersWithRegisterTrue, int TotalExaminersWithRegisterFalse, int TotalSelectedNotRegistered, int TotalSelectedInTrainerTab)> GetPaginatedAsync(
             int skip,
             int take,
             string searchValue,
             string sortColumn,
             string sortDirection,
             string sessionLevelFilter,
             string subjectFilter,
             string paperCodeFilter,
             string regionCodeFilte);
         Task DeleteAsync(int id);
        Task<ExaminerRecruitmentTrainingSelection> GetByExaminerRecruitmentIdAsync(int id);
        Task<(List<ExaminerRecruitmentTrainingSelectionViewModel> Data, int TotalCount)> GetDeselectedExaminersAsync(
            int skip,
            int take,
            string searchValue,
            string sortColumn,
            string sortDirection,
            string sessionLevelFilter,
            string subjectFilter,
            string paperCodeFilter,
            string regionCodeFilter);


        Task<(List<ExaminerRecruitmentAndSelectionViewModel> Data, int TotalCount, int TotalExaminersWithRegisterTrue, int TotalExaminersWithRegisterFalse, int TotalSelectedNotRegistered, int TotalSelectedInTrainerTab)> GetPaginatedRegisterAsync(
         int skip,
         int take,
         string searchValue,
         string sortColumn,
         string sortDirection,
         string subjectFilter,
         string paperCodeFilter,
         string experienceFilter);

        Task<int[]> GetIdsForBulkDownload(
               string Subject = null,
               string Experience = null,
               string PaperCode = null,
               string Region = null);

    }

  
}
