using DocumentFormat.OpenXml.Office2013.Excel;
using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class RegisterRepository : IRegisterRepository
    {
        private readonly ApplicationDbContext _context;
        public RegisterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ExaminerScriptsMarked> CheckExaminerRegister(string subkey)
        {
           var data = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(t =>  t.EMS_SUBKEY == subkey);

            return data;
        }




        

        public async Task<IEnumerable<ExaminerScriptsMarked>> GetAllRegister()
        {
            return await _context.EXAMINER_TRANSACTIONS.ToListAsync();
        }

        public async Task<IEnumerable<ExaminerScriptsMarked>> GetComponentRegister(string examCode, string subjectCode, string paperCode,string activity,string regionCode)
        {
            try
            {
                var examiners2 = await _context.EXAMINER_TRANSACTIONS
             .Include(et => et.Examiner)
             .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }

            var examiners = await _context.EXAMINER_TRANSACTIONS
                .Include(et => et.Examiner)  
                .ToListAsync();




            if (!string.IsNullOrEmpty(examCode))
            {
                examiners = examiners.Where(et => et.EMS_SUB_SUB_ID.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subjectCode))
            {
                examiners = examiners.Where(et => et.EMS_SUB_SUB_ID.Substring(3) == subjectCode).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                examiners = examiners.Where(et => et.EMS_PAPER_CODE == paperCode).ToList();
            }

            if (!string.IsNullOrEmpty(activity))
            {
                examiners = examiners.Where(et => et.EMS_ACTIVITY == activity).ToList();
            }

         

            if (!string.IsNullOrEmpty(regionCode))
            {
                examiners = examiners.Where(a => a.EMS_MARKING_REG_CODE == regionCode).ToList();
            }

            return examiners;
        }

        public async Task<IEnumerable<ExaminerScriptsMarked>> GetComponentRegister2(string examCode, string subjectCode, string paperCode, string activity, string regionCode, string venue)
        {
         

            var examiners = await _context.EXAMINER_TRANSACTIONS
                .Include(et => et.Examiner)
                .ToListAsync();



            if (!string.IsNullOrEmpty(examCode))
            {
                examiners = examiners.Where(et => et.EMS_SUB_SUB_ID.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subjectCode))
            {
                examiners = examiners.Where(et => et.EMS_SUB_SUB_ID.Substring(3) ==  subjectCode).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                examiners = examiners.Where(et => et.EMS_PAPER_CODE == paperCode).ToList();
            }

            if (!string.IsNullOrEmpty(activity))
            {
                examiners = examiners.Where(et => et.EMS_ACTIVITY == activity).ToList();
            }

            //if (!string.IsNullOrEmpty(venue))
            //{
            //    var subjectsAtVenue = await _context.SubjectVenue.Where(a => a.Venue == venue).ToListAsync();


            //    examiners = examiners.Where().ToList();
            //}


            if (!string.IsNullOrEmpty(regionCode))
            {
                examiners = examiners.Where(a => a.EMS_MARKING_REG_CODE == regionCode).ToList();
            }

            return examiners;
        }





        public async Task<ExaminerScriptsMarked> GetExaminer(string subKey)
        {
            return await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(t => t.EMS_SUBKEY == subKey);
        }

        public async Task MarkPresent(ExaminerScriptsMarked register,string userid)
        {

                _context.EXAMINER_TRANSACTIONS.Update(register);
            await _context.SaveChangesAsync(userid);

        }

        //public async Task UpdateRegister(ExaminerScriptsMarked register,string userId)
        //{
        //    try
        //    {
        //        // Validate input
        //        if (register == null)
        //        {
        //            throw new ArgumentNullException(nameof(register));
        //        }


        //        // Check if the register exists
        //        var existingRegister = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a => a.EMS_SUBKEY == register.EMS_SUBKEY);

        //        if (existingRegister != null)
        //        {

        //            // Update existing register conditionally
        //            //if (register.EMS_SUBKEY != null)
        //            //{
        //            //    existingRegister.EMS_SUBKEY = register.IDNumber;
        //            //}

        //            if (register.EMS_SUBKEY != null)
        //            {
        //                existingRegister.EMS_SUBKEY = register.EMS_SUBKEY;
        //            }

        //            //if (register.ExaminerCode != null)
        //            //{
        //            //    existingRegister.ExaminerCode = register.ExaminerCode;
        //            //}

        //            if (register.RegisterStatus != null)
        //            {
        //                existingRegister.RegisterStatus = register.RegisterStatus;
        //            }


        //            // Update other properties as needed

        //            _context.EXAMINER_TRANSACTIONS.Update(existingRegister);
        //        }
               



        //        // Save changes to the database
        //        await _context.SaveChangesAsync(userId);
        //    }

        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}



        //public async Task UpdateRegisterList(List<ExaminerScriptsMarked> registers, ApplicationUser applicationUser)
        //{
        //    foreach (var register in registers)
        //    {
        //        var existingRegister = await _context.ExaminerRegister
        //            .FirstOrDefaultAsync(r => r.IDNumber == register.EMS_NATIONAL_ID);

        //        if (existingRegister != null)
        //        {

        //            _context.ExaminerRegister.Update(existingRegister);
        //        }
        //        else
        //        {
        //            // Check if a record with the same IDNumber exists
        //            var idNumberExists = await _context.ExaminerRegister
        //                .AnyAsync(r => r.IDNumber == register.EMS_NATIONAL_ID);

        //            if (!idNumberExists)
        //            {

        //                var crt = new RegisterDto()
        //                {
        //                    EMS_SUBKEY = register.EMS_SUBKEY,
        //                    Status = "Absent",
        //                    ExaminerCode = register.EMS_EXAMINER_CODE,

        //                    IDNumber = register.EMS_NATIONAL_ID,
        //                    RecommendedDate = DateTime.Now.ToString(),
        //                    RecommendedBy = applicationUser.Id,
        //                    AttendanceStatus = "Pending",
        //                    AttendanceStatusBy = "system",
        //                    AttendanceUpdateDate = DateTime.Now.ToString(),
        //                };
        //                // Add the new record
                 
        //                _context.ExaminerRegister.Add(crt);
        //            }
        //            else
        //            {
        //                // Log or handle the duplicate IDNumber case
        //                // Optionally add a warning or skip to prevent errors
        //            }
        //        }
        //    }
        //    await _context.SaveChangesAsync(applicationUser.Id);
        //}


        public async Task<OperationResult> ConfirmRegister(ExaminerScriptsMarked register,string userId)
        {
            var available = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(t => t.EMS_SUBKEY == register.EMS_SUBKEY);

            if (available != null)
            {
                available.AttendanceStatus = register.AttendanceStatus;
                available.AttendanceStatusDate = register.AttendanceStatusDate;
                available.AttendanceStatusBy = register.AttendanceStatusBy;
               

                _context.EXAMINER_TRANSACTIONS.Update(available);
                await _context.SaveChangesAsync(userId);
                  return new OperationResult
                {
                    Success = true,
                    Message = "Thank you for Confimation"
                };
            }

            return new OperationResult
            {
                Success = true,
                Message = "Thank you for Confimation"
            };
        }
    }
}
