using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.Interfaces.Major
{
    public interface ITandSFilesRepository
    {
        Task<TandSFile> AddTandSFile(TandSFile tandSFile,string userid);

        Task<IEnumerable<TandSFile>> GetTandSFiles(string nationaId, string tandscode, string subKey, string examinerCode);
    }
}
