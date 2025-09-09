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
    public class PaperMarkingRateRepository : IPaperMarkingRateRepository
    {
        private readonly ApplicationDbContext _papercodecontext;

        public PaperMarkingRateRepository(ApplicationDbContext papercodecontext)
        {
            _papercodecontext = papercodecontext;
        }

        public async Task<IEnumerable<PaperMarkingRate>> GetAllPaperCodes()
        {
            var excludedDescriptions = new[] { "MULTIPLE CHOICE", "CONTINUOUS ASSESSMENT" };
            return await _papercodecontext.CAN_PAPER_MARKING_RATE
                //.Where(a => !excludedDescriptions.Contains(a.PPR_PAPER_DESC))
                .ToListAsync();
        }

        public async Task<IEnumerable<PaperMarkingRate>> GetPaperCodeById(string subjectid)
        {
            var excludedDescriptions = new[] { "MULTIPLE CHOICE", "CONTINUOUS ASSESSMENT" };
            return await _papercodecontext.CAN_PAPER_MARKING_RATE
                .Where(e => e.SUB_SUB_ID == subjectid)
                //&&
                //            !excludedDescriptions.Contains(e.PPR_PAPER_DESC))
                .ToListAsync();
        }



        public async Task<IEnumerable<PaperMarkingRate>> GetSubjectsByExamSession(string examcode)
        {
            // Null check for examcode
            if (examcode != null && examcode.Length < 4)
            {
                // Extract the exam session substring
                //string examSessionSubstring = examcode.Substring(0, examcode.Length-1);
                //string examSessionSubString = PPR_SUB_SUB_ID.Substring(PPR_SUB_SUB_ID.Length - 4, PPR_SUB_SUB_ID.Length - 1);

                // Fetch subjects based on the extracted exam session substring
                var subjects = await _papercodecontext.CAN_PAPER_MARKING_RATE
                //    .Where(s => s.SUB_SUB_ID.StartsWith(examcode))
                //    .OrderBy(s => s.SUB_SUBJECT_CODE)
                //    .Distinct()
                //    .ToListAsync();

                //return subjects;
                .Where(s => s.SUB_SUB_ID.StartsWith(examcode))
                      .Select(s => new PaperMarkingRate()
                      {
                          SUB_SUB_ID = s.SUB_SUB_ID,
                          SUB_SUBJECT_CODE = s.SUB_SUBJECT_CODE,
                          PPR_PAPER_DESC = s.PPR_PAPER_DESC,
                          SUB_SUBJECT_DESC = s.SUB_SUBJECT_DESC,
                          PPR_MARKING_RATE = s.PPR_MARKING_RATE,

                          // Add other properties you need
                      })
                      .Distinct()
                      .OrderBy(s => s.SUB_SUB_ID)
                      .ToListAsync();

                return subjects;
            }
            else
            {
                // If examcode is null or its length is less than 4, return an empty list
                return new List<PaperMarkingRate>();
            }
        }

        public async Task UpdatePaperMarkingRates(List<PaperMarkingRate> rates, string SUB_SUB_ID)
        {
            var sql = "UPDATE [dbo].[CAN_PAPER_MARKING_RATE] " +
                        "SET [PPR_MARKING_RATE] = @PaperMarkingRate " +
                        "WHERE SUB_SUB_ID = @PaperSubId AND PPR_PAPER_CODE = @PaperCode ";

            foreach (var rate in rates)
            {
                object[] paramItems = new object[]
                {
                    new SqlParameter("@PaperMarkingRate", rate.PPR_MARKING_RATE),
                    new SqlParameter("@PaperSubId", SUB_SUB_ID),
                    new SqlParameter("@PaperCode", rate.PPR_PAPER_CODE)
                };
                await _papercodecontext.Database.ExecuteSqlRawAsync(sql, paramItems);
            }
        }


        public async Task UpdatePaperMarkingRate(PaperMarkingRate updatedMarkingRates)
        {

            var paperToUpdate = await _papercodecontext.CAN_PAPER_MARKING_RATE
                .FirstOrDefaultAsync(e => e.SUB_SUB_ID == updatedMarkingRates.SUB_SUB_ID);

            if (paperToUpdate == null)
            {

                throw new Exception("Paper not found");
            }

            foreach (var propertyInfo in typeof(PaperMarkingRate).GetProperties())
            {
                if (propertyInfo.CanWrite && propertyInfo.Name != "Id")
                {
                    var updatedValue = typeof(PaperMarkingRate).GetProperty(propertyInfo.Name)?.GetValue(updatedMarkingRates);
                    if (updatedValue != null)
                    {
                        propertyInfo.SetValue(paperToUpdate, updatedValue);
                    }
                }
            }



            // Set the EntityState to Modified to indicate that the entity has been modified
            _papercodecontext.Entry(paperToUpdate).State = EntityState.Modified;

            // Save changes to the database
            await _papercodecontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<PaperMarkingRate>> GetPaperCodesBySubject(string subject, string examcode)
        {
            // Check if both subject and examcode are valid
            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(examcode))
            {
                // Define excluded descriptions
                var excludedDescriptions = new[] { "M", "C" };

                // Fetch filtered paper codes
                var paperCodes = await _papercodecontext.CAN_PAPER_MARKING_RATE
                    .Where(s => s.SUB_SUBJECT_CODE.StartsWith(subject) &&
                                s.SUB_SUB_ID.StartsWith(examcode))
                    //&&
                    //            !excludedDescriptions.Contains(s.PPR_PAPER_DESC))
                    .Select(s => new PaperMarkingRate
                    {
                        SUB_SUB_ID = s.SUB_SUB_ID,
                        SUB_SUBJECT_CODE = s.SUB_SUBJECT_CODE,
                        PPR_PAPER_DESC = s.PPR_PAPER_DESC,
                        PPR_PAPER_CODE = s.PPR_PAPER_CODE,
                        PPR_MARKING_RATE = s.PPR_MARKING_RATE
                        // Add other properties if needed
                    })
                    .Distinct()
                    .OrderBy(s => s.PPR_PAPER_CODE)
                    .ToListAsync();

                return paperCodes;
            }

            // Return an empty list if input is invalid
            return new List<PaperMarkingRate>();
        }


        public async Task<PaperMarkingRate> GetPaperMarkingRate(string subject, string papercode, string examcode)
        {
            var sub = examcode + subject;
            var data = await _papercodecontext.CAN_PAPER_MARKING_RATE.FirstOrDefaultAsync(e => e.PPR_PAPER_CODE == papercode && e.SUB_SUB_ID ==  sub);
            return data;
        }

        public async Task<List<PaperMarkingRate>> GetPaperCodeBySub(string subjectId)
        {
            // Validate input
            if (string.IsNullOrEmpty(subjectId))
            {
                return new List<PaperMarkingRate>();
            }

            // Exclude specific paper descriptions
            var excludedDescriptions = new[] { "M", "C" };

            // Fetch data
            var data = await _papercodecontext.CAN_PAPER_MARKING_RATE
                .Where(e => e.SUB_SUB_ID == subjectId) 
                //&&
                //            !excludedDescriptions.Contains(e.PPR_PAPER_DESC))
                .GroupBy(e => e.PPR_PAPER_CODE) // Group by paper code
                .Select(g => g.First())         // Select the first entry from each group
                .ToListAsync();

            return data;
        }


    }
}
