using DinkToPdf;

namespace ExaminerPaymentSystem.Interfaces.Common
{
    public interface IPdfService
    {
        byte[] ConvertPdf(string htmlcontent, Orientation? orientation = Orientation.Landscape, PaperKind? pk = PaperKind.A3);
    }
}
