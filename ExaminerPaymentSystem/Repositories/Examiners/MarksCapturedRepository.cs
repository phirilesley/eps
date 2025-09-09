using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class MarksCapturedRepository : IMarksCapturedRepository
    {
        private readonly ApplicationDbContext _context;
        public MarksCapturedRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task<IEnumerable<MarksCaptured>> GetAllMarksCaptured()
        {
            throw new NotImplementedException();
        }

        public async Task<MarksCaptured> GetMarkCapturedByParameters(string examcode, string subject, string papercode)
        {
            try
            {
                var data = await _context.EXM_SCRIPT_CAPTURED.FirstOrDefaultAsync(e => e.ExamCode == examcode && e.SubjectCode == subject && e.PaperCode == papercode);

                return data;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as per your application's requirements
                // For example, you can log the exception using a logging framework like Serilog or NLog
                // You can also throw a custom exception or return a default value
                // For now, let's just log the exception and return null
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<MarksCaptured> GetComponentMarkCaptured(string examCode, string subjectCode, string paperCode,string regionCode)
        {
            try
            {
                var scripData = new MarksCaptured();
                if (!string.IsNullOrEmpty(regionCode) && !string.IsNullOrEmpty(subjectCode))
                {
                    scripData = await _context.EXM_SCRIPT_CAPTURED.FirstOrDefaultAsync(e => e.ExamCode == examCode && e.SubjectCode == subjectCode.Substring(3) && e.PaperCode == paperCode && e.RegionCode == regionCode);
                }
                else
                {
                    if (!string.IsNullOrEmpty(subjectCode))
                    {
                        scripData = await _context.EXM_SCRIPT_CAPTURED.FirstOrDefaultAsync(e => e.ExamCode == examCode && e.SubjectCode == subjectCode.Substring(3) && e.PaperCode == paperCode);
                    }


                }
                return scripData;
            }
            catch (Exception)
            {

                throw;
            }
          
        }

        public async Task<IEnumerable<MarksCaptured>> GetMarkCapturedGrade7List(string examcode, string subjectcode)
        {
            var data = await _context.EXM_SCRIPT_CAPTURED.Where(e => e.ExamCode == examcode && e.SubjectCode == subjectcode).ToListAsync();
            return data;
        }

        public Task UpdateMarksCaptured(MarksCaptured capturedMarks)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PaperMarkingRate>> GetAllPaperCodes()
        {
            return await _context.CAN_PAPER_MARKING_RATE.ToListAsync();
        }

        public async Task<IEnumerable<PaperMarkingRate>> GetPaperCodeById(string subjectid)
        {
            return await _context.CAN_PAPER_MARKING_RATE.Where(e => e.SUB_SUB_ID == subjectid).ToListAsync();
        }





        public async Task UpdateExaminerScriptsMarked(RootObjectMarksCaptured rootObject)
        {
            var sql = "UPDATE [dbo].[EXM_SCRIPT_CAPTURED] " +
                       "SET [RegionCode] = @RegionCode, [TotalScriptsCaptured] = @TotalScriptsCaptured " +
                       "WHERE ExamCode = @ExamCode AND SubjectCode = @SubjectCode AND PaperCode = @PaperCode ";

            object[] paramItems = new object[]
            {
        new SqlParameter("@ExamCode", rootObject.Exam_Code),
        new SqlParameter("@SubjectCode", rootObject.SUBJECT_CODE),
        new SqlParameter("@PaperCode", rootObject.PAPER_CODE),
        new SqlParameter("@RegionCode", rootObject.Region_Code),
        new SqlParameter("@TotalScriptsCaptured", rootObject.TotalScriptsCaptured),
            };
            await _context.Database.ExecuteSqlRawAsync(sql, paramItems);
        }

        public async Task UpdateExaminerScripts(MarksCaptured updatedMarksCaptured)
        {

            var capturedMarkToUpdate = await _context.EXM_SCRIPT_CAPTURED
                .FirstOrDefaultAsync(e => e.Id == updatedMarksCaptured.Id);

            if (capturedMarkToUpdate == null)
            {

                throw new Exception("Mark not found");
            }

            foreach (var propertyInfo in typeof(MarksCaptured).GetProperties())
            {
                if (propertyInfo.CanWrite && propertyInfo.Name != "Id")
                {
                    var updatedValue = typeof(MarksCaptured).GetProperty(propertyInfo.Name)?.GetValue(updatedMarksCaptured);
                    if (updatedValue != null)
                    {
                        propertyInfo.SetValue(capturedMarkToUpdate, updatedValue);
                    }
                }
            }



            // Set the EntityState to Modified to indicate that the entity has been modified
            _context.Entry(capturedMarkToUpdate).State = EntityState.Modified;

            // Save changes to the database
            await _context.SaveChangesAsync();
        }



        public async Task<OperationResult> InsertExaminerTransactions(MarksCaptured rootObject,ApplicationUser applicationUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(rootObject.RegionCode))
                {
                    var existingRecord = await _context.EXM_SCRIPT_CAPTURED
.FirstOrDefaultAsync(e => e.SubjectCode == rootObject.SubjectCode
                       && e.PaperCode == rootObject.PaperCode
                       && e.ExamCode == rootObject.ExamCode && e.RegionCode == rootObject.RegionCode);

                    if (existingRecord != null)
                    {
                  
                            existingRecord.TotalScriptsCaptured = rootObject.TotalScriptsCaptured;
                            existingRecord.RegionCode = rootObject.RegionCode;
                            existingRecord.AccountsTotalScriptCaptured = rootObject.AccountsTotalScriptCaptured;
                            existingRecord.AbsentScripts = rootObject.AbsentScripts;
                            existingRecord.ApportionedScripts = rootObject.ApportionedScripts;
                            existingRecord.PirateCandidates = rootObject.PirateCandidates;
                            existingRecord.ScriptMarked = rootObject.ScriptMarked;
                            existingRecord.Exceptions = rootObject.Exceptions;
                        

                        _context.EXM_SCRIPT_CAPTURED.Update(existingRecord);
                    }

                    else
                    {
                        var newmarkcaptured = new MarksCaptured()
                        {
                            ExamCode = rootObject.ExamCode,
                            SubjectCode = rootObject.SubjectCode,
                            PaperCode = rootObject.PaperCode,
                            RegionCode = string.IsNullOrEmpty(rootObject.RegionCode) ? "00" : rootObject.RegionCode,
                            TotalScriptsCaptured = rootObject.TotalScriptsCaptured,
                            ApportionedScripts = rootObject.ApportionedScripts,
                            AbsentScripts = rootObject.AbsentScripts,
                            AccountsTotalScriptCaptured = rootObject.AccountsTotalScriptCaptured,
                            PirateCandidates = rootObject.PirateCandidates,
                            ScriptMarked = rootObject.ScriptMarked,
                            Exceptions = rootObject.Exceptions,
                        };


                        _context.EXM_SCRIPT_CAPTURED.Add(newmarkcaptured);

                    }
                }
                else
                {
                    var existingRecord = await _context.EXM_SCRIPT_CAPTURED
.FirstOrDefaultAsync(e => e.SubjectCode == rootObject.SubjectCode
                      && e.PaperCode == rootObject.PaperCode
                      && e.ExamCode == rootObject.ExamCode);

                    if (existingRecord != null)
                    {

                        existingRecord.TotalScriptsCaptured = rootObject.TotalScriptsCaptured;
                       
                        existingRecord.AccountsTotalScriptCaptured = rootObject.AccountsTotalScriptCaptured;
                        existingRecord.AbsentScripts = rootObject.AbsentScripts;
                        existingRecord.ApportionedScripts = rootObject.ApportionedScripts;
                        existingRecord.PirateCandidates = rootObject.PirateCandidates;
                        existingRecord.ScriptMarked = rootObject.ScriptMarked;
                        existingRecord.Exceptions = rootObject.Exceptions;


                        _context.EXM_SCRIPT_CAPTURED.Update(existingRecord);
                    }

                    else
                    {
                        var newmarkcaptured = new MarksCaptured()
                        {
                            ExamCode = rootObject.ExamCode,
                            SubjectCode = rootObject.SubjectCode,
                            PaperCode = rootObject.PaperCode,
                            RegionCode = string.IsNullOrEmpty(rootObject.RegionCode) ? "00" : rootObject.RegionCode,
                            TotalScriptsCaptured = rootObject.TotalScriptsCaptured,
                            ApportionedScripts = rootObject.ApportionedScripts,
                            AbsentScripts = rootObject.AbsentScripts,
                            AccountsTotalScriptCaptured = rootObject.AccountsTotalScriptCaptured,
                            PirateCandidates = rootObject.PirateCandidates,
                            ScriptMarked = rootObject.ScriptMarked,
                            Exceptions = rootObject.Exceptions,
                        };


                        _context.EXM_SCRIPT_CAPTURED.Add(newmarkcaptured);

                    }
                }

             

                await _context.SaveChangesAsync(applicationUser.Id);

                return new OperationResult
                {
                    Success = true,
                    Message = "Examiner added successfully"
                };
            }
            catch (Exception ex)
            {

                return new OperationResult
                {
                    Success = false,
                    Message = "An error occurred while adding the examiner. Please try again. " + ex.Message
                };
            }
           
        }

        //method to check examiner transactions
        public async Task<List<MarksCaptured>> CheckExaminerTransactions(string examinerCode, string subjectCode, string paperCode, string searchBmsCode)
        {


            try
            {
                var markscaptured = await _context.EXM_SCRIPT_CAPTURED
               .Where(e => e.ExamCode == examinerCode && e.SubjectCode == subjectCode && e.PaperCode == paperCode)
               .ToListAsync();
                return markscaptured;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        public async Task<IEnumerable<MarksCaptured>> GetMarksCaptured(string ExaminerCode, string subjectCode, string paperCode)
        {

            var marks = await _context.EXM_SCRIPT_CAPTURED
                  .Where(s => s.ExamCode == ExaminerCode && s.SubjectCode == subjectCode && s.PaperCode == paperCode)
                  .Select(s => new MarksCaptured()
                  {
                      ExamCode = s.ExamCode,
                      SubjectCode = s.SubjectCode,
                      PaperCode = s.PaperCode,
                    
                      TotalScriptsCaptured = s.TotalScriptsCaptured,
                      ApportionedScripts = s.ApportionedScripts,
                      AbsentScripts = s.AbsentScripts,
                      ScriptMarked = s.ScriptMarked,
                      PirateCandidates = s.PirateCandidates,    
                      AccountsTotalScriptCaptured = s.AccountsTotalScriptCaptured,
                      Exceptions = s.Exceptions,
                      
                     
                      // Add other properties you need
                  })
                  .Distinct()
                  .OrderBy(s => s.SubjectCode)
                  .ToListAsync();

            return marks;

        }

    }
}
