using DinkToPdf;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Mvc;
using System.Web;
namespace ExaminerPaymentSystem.Interfaces.Common
{
    public interface IExportService
    {
        Task<byte[]> ExportToPdf(string url, string reportTitle, string paperKind = "A3", Orientation or = Orientation.Portrait);

        Task<byte[]> ExportPageToPdf<T>(ReportGenerationViewModel<T> model);

        FileStreamResult ExportToExcel(IEnumerable<object> data, string filename);

        FileStreamResult ExportToExcel<T>(List<T> listdata, string filename);

    }
}
