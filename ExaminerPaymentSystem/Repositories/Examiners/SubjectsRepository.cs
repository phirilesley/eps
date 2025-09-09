using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExaminerPaymentSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class SubjectsRepository: ISubjectsRepository
    {
        private readonly ApplicationDbContext _subjectscontext;

        public SubjectsRepository(ApplicationDbContext subjectscontext)
        {
            _subjectscontext = subjectscontext;
        }

        public async Task<IEnumerable<Subjects>> GetAllPaperCodes()
        {
            return await _subjectscontext.Subjects.ToListAsync();
        }

        public async Task<IEnumerable<Subjects>> GetPaperCodeById(string subjectid)
        {
            return await _subjectscontext.Subjects.Where(e => e.SUB_SUB_ID == subjectid).ToListAsync();
        }

        public async Task<Subjects> GetSubjectCode(string subjectCode)
        {
            return await _subjectscontext.Subjects.FirstOrDefaultAsync(e => e.SUB_SUBJECT_CODE == subjectCode);
        }



        public async Task<IEnumerable<Subjects>> GetSubjectsByPrefixes(string[] prefixes)
        {
            return await _subjectscontext.Subjects
                .Where(subject => prefixes.Any(prefix => subject.SUB_SUBJECT_CODE.StartsWith(prefix)))
                .ToListAsync();
        }


        public async Task<List<Subjects>> GetSubjectsByPrefix(string prefix)
        {
            return await _subjectscontext.Subjects
          .Where(s => s.SUB_SUBJECT_CODE.StartsWith(prefix)) // Assuming 'Code' holds the subject identifier
          .ToListAsync();
        }


        public async Task<IEnumerable<(string SubSubjectCode, string SubSubjectDesc)>> GetSubjectByLevel(string prefix)
        {
            return await _subjectscontext.CAN_PAPER_MARKING_RATE
                .Where(s => s.SUB_SUBJECT_CODE.StartsWith(prefix))
                .Select(s => new ValueTuple<string, string>(s.SUB_SUBJECT_CODE, s.SUB_SUBJECT_DESC))
                .Distinct()
                .ToListAsync();
        }


        public async Task<IEnumerable<(string SubSubjectCode, string SubSubjectDesc)>> GetSubjectByLevels(string[] prefixes)
        {
            return await _subjectscontext.CAN_PAPER_MARKING_RATE
                .Where(s => prefixes.Any(prefix => s.SUB_SUBJECT_CODE.StartsWith(prefix)))
                .Select(s => new ValueTuple<string, string>(s.SUB_SUBJECT_CODE, s.SUB_SUBJECT_DESC))
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetPaperCodesBySubjectCode(string subjectCode)
        {
            return await _subjectscontext.CAN_PAPER_MARKING_RATE
                .Where(s => s.SUB_SUBJECT_CODE == subjectCode)
                .Select(s => s.PPR_PAPER_CODE) // Ensure this is the correct column name
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Subjects>> GetSubjectsByExamSession(string examcode)
        {
            // Null check for examcode
            if (examcode != null && examcode.Length < 4)
            {
               

                // Fetch subjects based on the extracted exam session substring
                var subjects = await _subjectscontext.Subjects
                    .Where(s => s.SUB_SUB_ID.StartsWith(examcode))
                    //.OrderBy(s => s.SUBJECT_CODE)
                    //.Distinct()
                    .ToListAsync();

                return subjects;
                //.Where(s => s.PPR_SUB_SUB_ID.StartsWith(examcode))
                //      .Select(s => new PaperMarkingRate()
                //      {
                //          PPR_SUB_SUB_ID = s.PPR_SUB_SUB_ID,
                //          SUBJECT_CODE = s.SUBJECT_CODE,
                //          PPR_PAPER_DESC = s.PPR_PAPER_DESC,
                //          PPR_PAPER_CODE = s.PPR_PAPER_CODE,
                //          PPR_MARKING_RATE = s.PPR_MARKING_RATE,

                //          // Add other properties you need
                //      })
                //      .Distinct()
                //      .OrderBy(s => s.PPR_PAPER_CODE)
                //      .ToListAsync();

                return subjects;
            }
            else
            {
                // If examcode is null or its length is less than 4, return an empty list
                return new List<Subjects>();
            }
        }

    
    }
}
