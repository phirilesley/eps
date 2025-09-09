using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Repositories.Examiners;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IReportRepository
    {
        Task<ReportDataResult> GetFilteredData(string filterStatus, string venue, string activity, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser);

        Task<ReportAccountsEPSDataResult> GetFilteredAccountsEPSData(string filterStatus, string venue, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser);

        Task<ReportAccountsDataResult> GetFilteredAccountsData(string filterStatus, string venue, string activity, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser);
        Task<ReportScriptsDataResult> GetScriptsFilteredData(string filterStatus, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser);

        Task<ReportRegisterDataResult> GetRegisterFilteredData(string filterStatus, string activity, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser);
    }
}
