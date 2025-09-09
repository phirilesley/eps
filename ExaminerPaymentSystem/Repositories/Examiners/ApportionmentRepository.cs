using ExaminerPaymentSystem.Data;
using Microsoft.EntityFrameworkCore;
using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;


namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class ApportionmentRepository: IApportionmentRepository
    {
        private readonly ApplicationDbContext _candidatesContext;

        public ApportionmentRepository(ApplicationDbContext candidatesContext)
        {
            _candidatesContext = candidatesContext;
        }

        public async Task<IEnumerable<Apportionment>> GetNumberOfCandidatesBySubjectComponent(string subjectCode, string paperCode)
        {
            var apportionments = await _candidatesContext.APPORTIONMENT
                .Where(a => a.MKS_SUBJECT_CODE == subjectCode && a.MKS_PAPER_CODE == paperCode)
                .Select(a => new Apportionment()
                {

                    NUMBER_OF_CANDIDATES = a.NUMBER_OF_CANDIDATES,
                })
                .ToListAsync();

            return apportionments;
        }

        public async Task<IEnumerable<ExaminerScriptsMarked>> GetNumberOfScriptsMarked(string subjectCode, string paperCode)
        {
            var scriptsmarked = new List<ExaminerScriptsMarked>();
            //var scriptsmarked = await _candidatesContext.EXAMINER_TRANSACTIONS
            //    .Where(a => a.EMS_SUBJECT_CODE == subjectCode && a.EMS_PAPER_CODE == paperCode)
            //    .Select(a => new ExaminerScriptsMarked()
            //    {

            //        SCRIPTS_MARKED = a.SCRIPTS_MARKED,
            //    })
            //    .ToListAsync();

            return scriptsmarked;
        }

    }
}
