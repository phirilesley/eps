using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class TandSRepository : ITandSRepository
    {
        private readonly ApplicationDbContext _context;

        public TandSRepository(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<TandS> AddTandS(TandS tands,string userId)
        {
            _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Add(tands);
            await _context.SaveChangesAsync(userId);
            return tands;
        }

        public async Task UpdateTandS(TandS tands, string userid)
        {
            var existingEntity = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(c => c.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID && c.TANDSCODE == tands.TANDSCODE);
            if (existingEntity != null)
            {
      

    
                existingEntity.ADJ_TOTAL = tands.ADJ_TOTAL;
                existingEntity.ADJ_BY = tands.ADJ_BY;
                existingEntity.DATE_ADJ = tands.DATE_ADJ;
         
                existingEntity.ACCOUNTS_DATE = tands.ACCOUNTS_DATE;
                existingEntity.ACCOUNTS_STATUS = tands.ACCOUNTS_STATUS;
                existingEntity.ACCOUNTS_STATUS_BY = tands.ACCOUNTS_STATUS_BY;
                 existingEntity.ReturnBackStatus = tands.ReturnBackStatus;
                existingEntity.ReturnComment = tands.ReturnComment;
                existingEntity.ReturnBackBy = tands.ReturnBackBy;
                existingEntity.ACCOUNTS_REVIEW = "Pending";
                existingEntity.ACCOUNTS_REVIEW_BY = tands.ACCOUNTS_STATUS_BY;
                existingEntity.ACCOUNTS_REVIEW_DATE = tands.ACCOUNTS_REVIEW_BY;


                _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Update(existingEntity);
                await _context.SaveChangesAsync(userid);
            }
        }



        public async Task<TandS> AdjustTandS(TandS data,string userId)
        {
            var existingEntity = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(c => c.EMS_NATIONAL_ID == data.EMS_NATIONAL_ID && c.EMS_SUBKEY == data.EMS_SUBKEY);
            if (existingEntity != null)
            {

         

                if (data.EMS_SUBKEY != null)
                {
                    existingEntity.EMS_SUBKEY = data.EMS_SUBKEY;
                }

          

                _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Update(existingEntity);
                await _context.SaveChangesAsync(userId);
            }
            return existingEntity;
        }

        public async Task AdjustAdvance(TandS data, string code,string userid)
        {

            var advance = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.FirstOrDefaultAsync(a => a.TANDSCODE == code && a.EMS_NATIONAL_ID == data.EMS_NATIONAL_ID);
            if (advance != null)
            {

             
                if (data.EMS_SUBKEY != null)
                {
                    advance.EMS_SUBKEY = data.EMS_SUBKEY;
                }

             


                _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Update(advance);

                await _context.SaveChangesAsync(userid);
            }

        }

            public async Task AdjustTandsDetails(TandS data, string code,string userId)
            {
                var details = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                .Where(t => t.TANDSCODE == code  && t.EMS_NATIONAL_ID == data.EMS_NATIONAL_ID)
                .ToListAsync();

                foreach (var item in details)
                {
                 

                    if (data.EMS_SUBKEY != null)
                    {
                        item.EMS_SUBKEY = data.EMS_SUBKEY;
                    }

             
                }

                _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.UpdateRange(details);
            await _context.SaveChangesAsync(userId);
        }

        public async Task AdjustTandSFiles(TandS data, string code, string userId)
        {
                var files = await _context.TRAVELLING_AND_SUBSISTENCE_FILES
    .Where(t => t.TANDSCODE == code && t.EMS_NATIONAL_ID == data.EMS_NATIONAL_ID)
    .ToListAsync();
                foreach (var item in files)
                {
                   

                    if (data.EMS_SUBKEY != null)
                    {
                        item.EMS_SUBKEY = data.EMS_SUBKEY;
                    }

                }
                _context.TRAVELLING_AND_SUBSISTENCE_FILES.UpdateRange(files);
            await _context.SaveChangesAsync(userId);


        }




        public async Task<TandS> GetOneTandS(string idNumber,string tandsCode,string subKey)
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                                 .Include(t => t.Examiner)
                                  .Include(t => t.Examiner.ExaminerScriptsMarkeds)
                                 .Include(t => t.TandSDetails)
                                 .Include(t => t.TandSAdvance)
                                 .Include(t => t.TandSFiles)
                                 .FirstOrDefaultAsync(t => t.EMS_NATIONAL_ID == idNumber && t.TANDSCODE == tandsCode && t.EMS_SUBKEY == subKey);
        }

        public async Task<TandS> GetUserTandS(string idNumber,string subkey)
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                                 .Include(t => t.Examiner)
                                 .Include(t => t.TandSDetails)
                                 .Include(t => t.TandSAdvance)
                                 .Include(t => t.TandSFiles)
                                 .FirstOrDefaultAsync(t => t.EMS_NATIONAL_ID == idNumber && t.EMS_SUBKEY == subkey);
        }


        public async Task<List<TandS>> GetTandSsByNationalID(string idNumber)
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                                 .Include(t => t.Examiner)
                                 .Include(t => t.TandSDetails)
                                 .Include(t => t.TandSAdvance)
                                 .Include(t => t.TandSFiles)
                                 .Where(t => t.EMS_NATIONAL_ID == idNumber).ToListAsync();
        }

        public async Task DeleteTandS(string nationalId,string subkey,ApplicationUser applicationUser)
        {
            var tands = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == nationalId  && e.EMS_SUBKEY ==subkey);
            if (tands != null)
            {
             
                _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Remove(tands);
            

                var advance = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.FirstOrDefaultAsync(a => a.TANDSCODE == tands.TANDSCODE && a.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID);
                if (advance != null)
                {
                    _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Remove(advance);
                }

                var details = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                    .Where(a => a.TANDSCODE == tands.TANDSCODE &&  tands.EMS_NATIONAL_ID == nationalId)
                    .ToListAsync();
                _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.RemoveRange(details);

                var files = await _context.TRAVELLING_AND_SUBSISTENCE_FILES
                    .Where(a => a.TANDSCODE == tands.TANDSCODE && tands.EMS_NATIONAL_ID == nationalId)
                    .ToListAsync();
                _context.TRAVELLING_AND_SUBSISTENCE_FILES.RemoveRange(files);

                //var newdeleted = new DeletedTandS()
                //{
                //    IdNumber = nationalId,
                //    SubKey = tands.EMS_SUBKEY,
                //    ExaminerCode = tands.EMS_EXAMINER_CODE,
                //    TANDSCODE = tands.TANDSCODE,
                //   Comment ="Deleted Claim",
                   

                //    DeletedOrRejectedBy = applicationUser.UserName,
                //};

                ////newdeleted.Amount = decimal.Parse(tands.EMS_TOTAL);
                //_context.DeletedTandS.Add(newdeleted);

                await _context.SaveChangesAsync(applicationUser.Id);
            }
        }


        public async Task DeleteAllExaminerTandS(string nationalId, ApplicationUser applicationUser)
        {
            var tandsList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Where(e => e.EMS_NATIONAL_ID == nationalId).ToListAsync();

            foreach (var tands in tandsList)
            {
                if (tands != null)
                {

                    _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Remove(tands);


                    var advance = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.FirstOrDefaultAsync(a => a.TANDSCODE == tands.TANDSCODE && a.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID);
                    if (advance != null)
                    {
                        _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Remove(advance);
                    }

                    var details = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                        .Where(a => a.TANDSCODE == tands.TANDSCODE && tands.EMS_NATIONAL_ID == nationalId)
                        .ToListAsync();
                    _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.RemoveRange(details);

                    var files = await _context.TRAVELLING_AND_SUBSISTENCE_FILES
                        .Where(a => a.TANDSCODE == tands.TANDSCODE && tands.EMS_NATIONAL_ID == nationalId)
                        .ToListAsync();
                    _context.TRAVELLING_AND_SUBSISTENCE_FILES.RemoveRange(files);

                    //var newdeleted = new DeletedTandS()
                    //{
                    //    IdNumber = nationalId,
                    //    SubKey = tands.EMS_SUBKEY,
                    //    ExaminerCode = tands.EMS_EXAMINER_CODE,
                    //    TANDSCODE = tands.TANDSCODE,
                    //   Comment ="Deleted Claim",


                    //    DeletedOrRejectedBy = applicationUser.UserName,
                    //};

                    ////newdeleted.Amount = decimal.Parse(tands.EMS_TOTAL);
                    //_context.DeletedTandS.Add(newdeleted);

                    await _context.SaveChangesAsync(applicationUser.Id);
                }
            }

            
        }

        public async Task<IEnumerable<TandS>> GetAllTandS()
        {
            try
            {
                return await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                    .Include(a => a.Examiner)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
                .ToListAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }


  

        public async Task<IEnumerable<TandS>> GetAllTandSByComponent(string examCode, string subjectCode, string paperCode, string venue,string regionCode,string activity,List<string> roles)
        {
            try
            {
                List<TandS> tandSList = new List<TandS>();


                if (!string.IsNullOrEmpty(regionCode))
                {
                    tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                      .Where(s => s.EMS_VENUE == venue &&
                                  _context.EXAMINER_TRANSACTIONS
                                      .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present" && a.EMS_MARKING_REG_CODE == regionCode))
                      .Include(a => a.Examiner)
                      .Include(a => a.Examiner.ExaminerScriptsMarkeds)
                      .ToListAsync();

                }
                else
                {
                    tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                   .Where(s => s.EMS_VENUE == venue &&
                               _context.EXAMINER_TRANSACTIONS
                                   .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present"))
                   .Include(a => a.Examiner)
                   .ToListAsync();

                }
            



                if (!string.IsNullOrEmpty(activity))
            {
                tandSList = tandSList.Where(a => a.EMS_PURPOSEOFJOURNEY == activity).ToList();
            }




            // Apply filters separately
            if (!string.IsNullOrEmpty(examCode))
            {
                tandSList = tandSList.Where(a => a.EMS_SUBKEY.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subjectCode))
            {
                tandSList = tandSList.Where(a => a.EMS_SUBKEY.Length >= 7 &&
                                                a.EMS_SUBKEY.Substring(3, 4) == subjectCode).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                tandSList = tandSList.Where(a => a.EMS_SUBKEY.Length >= 9 &&
                                                a.EMS_SUBKEY.Substring(7, 2) == paperCode).ToList();
            }




            return tandSList;
            }
            catch (Exception ex)
            {
                // Improved exception handling with logging
               
                throw;
            }
        }


        public async Task<IEnumerable<TandS>> GetAllTandSByComponent2(string examCode,string subjectCode,string paperCode, string venue, string regionCode, string activity, List<string> roles)
        {
            try
            {
                List<TandS> tandSList = new List<TandS>();

                if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                {
                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                          .Where(s => s.EMS_VENUE == venue &&
                                      _context.EXAMINER_TRANSACTIONS
                                          .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.EMS_MARKING_REG_CODE == regionCode))
                          .Include(a => a.Examiner)
                          .ToListAsync();

                    }
                    else
                    {
                        tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                       .Where(s => s.EMS_VENUE == venue &&
                                   _context.EXAMINER_TRANSACTIONS
                                       .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY))
                       .Include(a => a.Examiner)
                       .ToListAsync();

                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                          .Where(s => s.EMS_VENUE == venue &&
                                      _context.EXAMINER_TRANSACTIONS
                                          .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present" && a.EMS_MARKING_REG_CODE == regionCode))
                          .Include(a => a.Examiner)

                          .ToListAsync();

                    }
                    else
                    {
                        tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                       .Where(s => s.EMS_VENUE == venue &&
                                   _context.EXAMINER_TRANSACTIONS
                                       .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present"))
                       .Include(a => a.Examiner)
                       .ToListAsync();

                    }
                }



                if (!string.IsNullOrEmpty(activity))
                {
                    tandSList = tandSList.Where(a => a.EMS_PURPOSEOFJOURNEY == activity).ToList();
                }

            


                // Apply filters separately
                if (!string.IsNullOrEmpty(examCode))
                {
                    tandSList = tandSList.Where(a => a.EMS_SUBKEY.StartsWith(examCode)).ToList();
                }

                if (!string.IsNullOrEmpty(subjectCode))
                {
                    tandSList = tandSList.Where(a => a.EMS_SUBKEY.Length >= 7 &&
                                                    a.EMS_SUBKEY.Substring(3, 4) == subjectCode).ToList();
                }

                if (!string.IsNullOrEmpty(paperCode))
                {
                    tandSList = tandSList.Where(a => a.EMS_SUBKEY.Length >= 9 &&
                                                    a.EMS_SUBKEY.Substring(7, 2) == paperCode).ToList();
                }


                return tandSList;
            }
            catch (Exception ex)
            {
                // Improved exception handling with logging

                throw;
            }
        }


        public async Task<IEnumerable<TandS>> GetAllAppliedTandS(string examCode, string subjectCode, string paperCode, string venue, string regionCode, string activity, List<string> roles)
        {
            try
            {
                List<TandS> tandSList = new List<TandS>();

         
                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                          .Where(s => s.EMS_VENUE == venue &&
                                      _context.EXAMINER_TRANSACTIONS
                                          .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.EMS_MARKING_REG_CODE == regionCode))
                          .Include(a => a.Examiner)
                          .ToListAsync();

                    }
                    else
                    {
                        tandSList = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                       .Where(s => s.EMS_VENUE == venue &&
                                   _context.EXAMINER_TRANSACTIONS
                                       .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY))
                       .Include(a => a.Examiner)
                       .ToListAsync();

                    }
            
                



                if (!string.IsNullOrEmpty(activity))
                {
                    tandSList = tandSList.Where(a => a.EMS_PURPOSEOFJOURNEY == activity).ToList();
                }




                // Apply filters separately
                if (!string.IsNullOrEmpty(examCode))
                {
                    tandSList = tandSList.Where(a => a.EMS_SUBKEY.StartsWith(examCode)).ToList();
                }

                if (!string.IsNullOrEmpty(subjectCode))
                {
                    tandSList = tandSList.Where(a => a.EMS_SUBKEY.Length >= 7 &&
                                                    a.EMS_SUBKEY.Substring(3, 4) == subjectCode).ToList();
                }

                if (!string.IsNullOrEmpty(paperCode))
                {
                    tandSList = tandSList.Where(a => a.EMS_SUBKEY.Length >= 9 &&
                                                    a.EMS_SUBKEY.Substring(7, 2) == paperCode).ToList();
                }


                return tandSList;
            }
            catch (Exception ex)
            {
                // Improved exception handling with logging

                throw;
            }
        }




        public async Task<IEnumerable<DeletedTandS>> GetAllDeleted()
        {
            return await _context.DeletedTandS.ToListAsync();
        }








   

        public async Task ApproveTandS(TandS tands,string role,string userid)
        {
            var existingEntity = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(c => c.TANDSCODE == tands.TANDSCODE  && c.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID && c.EMS_SUBKEY == tands.EMS_SUBKEY);
            if (existingEntity != null)
            {
            
                if(role == "SubjectManager")
                {
                    existingEntity.SUBJECT_MANAGER_STATUS = tands.SUBJECT_MANAGER_STATUS;
                    existingEntity.SUBJECT_MANAGER_STATUS_BY = tands.SUBJECT_MANAGER_STATUS_BY;
                    existingEntity.SUBJECT_MANAGER_DATE = tands.SUBJECT_MANAGER_DATE;
                    existingEntity.SUBJECT_MANAGER_COMMENT = tands.SUBJECT_MANAGER_COMMENT;
                    existingEntity.CENTRE_SUPERVISOR_STATUS = tands.CENTRE_SUPERVISOR_STATUS;
                    existingEntity.CENTRE_SUPERVISOR_STATUS_BY = tands.CENTRE_SUPERVISOR_STATUS_BY;
                    existingEntity.CENTRE_SUPERVISOR_DATE = tands.CENTRE_SUPERVISOR_DATE;
                    existingEntity.CENTRE_SUPERVISOR_COMMENT = tands.CENTRE_SUPERVISOR_COMMENT;

                }
                else if (role == "CentreSupervisor")
                {
                    existingEntity.SUBJECT_MANAGER_STATUS = tands.SUBJECT_MANAGER_STATUS;
                    existingEntity.SUBJECT_MANAGER_STATUS_BY = tands.SUBJECT_MANAGER_STATUS_BY;
                    existingEntity.SUBJECT_MANAGER_DATE = tands.SUBJECT_MANAGER_DATE;
                    existingEntity.SUBJECT_MANAGER_COMMENT = tands.SUBJECT_MANAGER_COMMENT;
                    existingEntity.CENTRE_SUPERVISOR_STATUS = tands.CENTRE_SUPERVISOR_STATUS;
                    existingEntity.CENTRE_SUPERVISOR_STATUS_BY = tands.CENTRE_SUPERVISOR_STATUS_BY;
                    existingEntity.CENTRE_SUPERVISOR_DATE = tands.CENTRE_SUPERVISOR_DATE;
                    existingEntity.CENTRE_SUPERVISOR_COMMENT = tands.CENTRE_SUPERVISOR_COMMENT;
                }

                else if (role == "PeerReviewer")
                {
                    if (tands.ACCOUNTS_REVIEW == "Pending" && tands.ACCOUNTS_STATUS == "Pending") {
                        existingEntity.ACCOUNTS_REVIEW = tands.ACCOUNTS_REVIEW;
                        existingEntity.ACCOUNTS_REVIEW_BY = tands.ACCOUNTS_REVIEW_BY;
                        existingEntity.ACCOUNTS_REVIEW_DATE = tands.ACCOUNTS_REVIEW_DATE;
                        existingEntity.ACCOUNTS_REVIEW_COMMENT = tands.ACCOUNTS_REVIEW_COMMENT;
                        existingEntity.ACCOUNTS_STATUS = tands.ACCOUNTS_STATUS;
                        existingEntity.ACCOUNTS_STATUS_BY = tands.ACCOUNTS_STATUS_BY;
                        existingEntity.ACCOUNTS_DATE = tands.ACCOUNTS_DATE;
                        existingEntity.ReturnBackBy = tands.ReturnBackBy;
                        existingEntity.ReturnBackStatus = tands.ReturnBackStatus;
                        existingEntity.ReturnComment = tands.ReturnComment;
                        existingEntity.ReturnDate = tands.ReturnDate;
                       
                    }
                    else {
                        existingEntity.ACCOUNTS_REVIEW = tands.ACCOUNTS_REVIEW;
                        existingEntity.ACCOUNTS_REVIEW_BY = tands.ACCOUNTS_REVIEW_BY;
                        existingEntity.ACCOUNTS_REVIEW_DATE = tands.ACCOUNTS_REVIEW_DATE;
                        existingEntity.ACCOUNTS_REVIEW_COMMENT = tands.ACCOUNTS_REVIEW_COMMENT;
                        existingEntity.STATUS = "Approved";

                    }
                }

                else if (role == "AssistantAccountant")
                {
                    existingEntity.STATUS= tands.STATUS;
                    existingEntity.STATUS_BY = tands.STATUS_BY;
                    existingEntity.STATUS_DATE = tands.STATUS_DATE;
                    existingEntity.STATUS_COMMENT = tands.STATUS_COMMENT;
                }

                _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Update(existingEntity);
                await _context.SaveChangesAsync(userid);
            }


        }
        public async Task RejectTandS(TandS tandss,string role,ApplicationUser applicationUser, string comment)
        {
            var tands = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == tandss.EMS_NATIONAL_ID && e.TANDSCODE == tandss.TANDSCODE);
            if (tands != null)
            {

                _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Remove(tands);
         

                var advance = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.FirstOrDefaultAsync(a => a.TANDSCODE == tands.TANDSCODE  && a.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID);
                if (advance != null)
                {
                    _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Remove(advance);
                }
                var details = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                    .Where(a => a.TANDSCODE == tands.TANDSCODE  && a.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID)
                    .ToListAsync();
                _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.RemoveRange(details);
                var files = await _context.TRAVELLING_AND_SUBSISTENCE_FILES
                    .Where(a => a.TANDSCODE == tands.TANDSCODE && a.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID)
                    .ToListAsync();
                _context.TRAVELLING_AND_SUBSISTENCE_FILES.RemoveRange(files);

                //var newdeleted = new DeletedTandS()
                //{
                //    IdNumber = tands.EMS_NATIONAL_ID,
                //    SubKey = tands.EMS_SUBKEY,
                //    ExaminerCode = tands.EMS_EXAMINER_CODE,
                //    TANDSCODE = tands.TANDSCODE,
                //    Comment = comment,


                //    DeletedOrRejectedBy = applicationUser.UserName,

                //};
                ////newdeleted.Amount = decimal.Parse(tands.EMS_TOTAL);
                //_context.DeletedTandS.Add(newdeleted);

                await _context.SaveChangesAsync(applicationUser.Id);
            }
        }

        public async Task ChangeTandS(string tandscode, string idnumber, string subKey,string examinerCode)
        {
            var existingEntity = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(c => c.EMS_NATIONAL_ID == idnumber && c.TANDSCODE == tandscode );
            if (existingEntity != null)
            {
                _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Remove(existingEntity);
                await _context.SaveChangesAsync();

              var tansdetails = await   _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                .Where(t => t.EMS_NATIONAL_ID == idnumber && t.TANDSCODE == tandscode )
                .ToListAsync();

                foreach (var item in tansdetails)
                {
                    _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.Remove(item);
                    await _context.SaveChangesAsync();

                }

                var advance = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.FirstOrDefaultAsync(c => c.EMS_NATIONAL_ID == idnumber && c.TANDSCODE == tandscode );

                if( advance != null )
                {
                    _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Remove(advance);
                    await _context.SaveChangesAsync();
                }

                var files = await _context.TRAVELLING_AND_SUBSISTENCE_FILES.Where(c => c.EMS_NATIONAL_ID == idnumber && c.TANDSCODE == tandscode ).ToListAsync();

                foreach (var file in files) {
                    _context.TRAVELLING_AND_SUBSISTENCE_FILES.Remove(file);
                    await _context.SaveChangesAsync();
                }
            }
        }


        public async Task<IEnumerable<TandS>> GetTandSComponentReport(string examcode, string subjectcode, string papercode, string venue, string activity, string regionCode)
        {
            var results = new List<TandS>();
            var subkeyPrefix = subjectcode + papercode + activity;

            if (!string.IsNullOrEmpty(regionCode))
            {
                results = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
              .Where(et => et.EMS_SUBKEY.StartsWith(subkeyPrefix) && et.EMS_VENUE == venue && (et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "BT" || et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "PBT") &&
                _context.EXAMINER_TRANSACTIONS
                                    .Any(a => a.EMS_SUBKEY == et.EMS_SUBKEY && a.RegisterStatus == "Present" && a.EMS_MARKING_REG_CODE == regionCode))
              .Include(a => a.Examiner)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
        .ToListAsync();

            }
            else
            {
                results = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
              .Where(et => et.EMS_SUBKEY.StartsWith(subkeyPrefix) && et.EMS_VENUE == venue && (et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "BT" || et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "PBT") &&
                _context.EXAMINER_TRANSACTIONS
                                    .Any(a => a.EMS_SUBKEY == et.EMS_SUBKEY && a.RegisterStatus == "Present"))
              .Include(a => a.Examiner)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
        .ToListAsync();

            }
            return results;
        }

        public async Task<IEnumerable<TandS>> GetVenueTandSReport(string examcode, string venue, string activity, List<string> year)
        {
            var results = new List<TandS>();
       

            results = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
              .Where(et => et.EMS_VENUE == venue && et.EMS_PURPOSEOFJOURNEY == activity && (et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "BT" || et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "PBT") &&
                             year.Any(session => et.EMS_SUBKEY.StartsWith(session)) &&
                _context.EXAMINER_TRANSACTIONS
                                    .Any(a => a.EMS_SUBKEY == et.EMS_SUBKEY && a.RegisterStatus == "Present" && a.EMS_ACTIVITY == activity))
              .Include(a => a.Examiner)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
        .ToListAsync();

            
            return results;

        }

        public async Task<IEnumerable<TandS>> GetActivityTandSReport(string examCode, string activity, List<string> year)
        {
            var results = new List<TandS>();
         
            results = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                .Where(et => et.EMS_PURPOSEOFJOURNEY == activity &&
                             et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "BT" &&
                              et.Examiner.EMS_ECT_EXAMINER_CAT_CODE != "PBT" &&
                             year.Any(session => et.EMS_SUBKEY.StartsWith(session)) && // Filter by EMS_SUBKEY starting with session
                             _context.EXAMINER_TRANSACTIONS
                                 .Any(a => a.EMS_SUBKEY == et.EMS_SUBKEY &&
                                           a.RegisterStatus == "Present" &&
                                           a.EMS_ACTIVITY == activity))
                .Include(a => a.Examiner)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
                .ToListAsync();



            return results;
        }


    }


    public class TandSDetailRepository : ITandSDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public TandSDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTandSDetail(TandSDetail tands,string userId)
        {
            _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.Add(tands);
            await _context.SaveChangesAsync(userId);
        }

        public async Task AddTandSAdvance(TandSAdvance tandsAdvance,string userId)
        {
            _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Add(tandsAdvance);
            await _context.SaveChangesAsync(userId);
        }

        public async Task UpdateTandSDetail(List<TandSDetail> tandsList,string userId)
        {
            foreach (var tands in tandsList)
            {
                var existingEntity = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID && e.TANDSCODE == tands.TANDSCODE && e.Id == tands.Id);
                if (existingEntity != null)
                {
                  
                    existingEntity.ADJ_BUSFARE =tands.ADJ_BUSFARE;
                    existingEntity.ADJ_ACCOMMODATION = tands.ADJ_ACCOMMODATION;
                    existingEntity.ADJ_BY = tands.ADJ_BY;
                    existingEntity.ADJ_TOTAL = tands.ADJ_TOTAL;
                    existingEntity.ADJ_DINNER = tands.ADJ_DINNER;
                    existingEntity.ADJ_LUNCH = tands.ADJ_LUNCH;
                    
                    _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.Update(existingEntity);
                }
            }

            await _context.SaveChangesAsync(userId);
        }


        public async Task DeleteTandSDetail(int id)
        {
            var tands = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.FindAsync(id);
            if (tands != null)
            {
                _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.Remove(tands);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TandSDetail>> GetAllTandSDetails()
        {
            return await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS.ToListAsync();
        }

    

        public async Task<List<TandSDetail>> GetTandSDetails(string nationalId, string tandsCode, string subKey, string examinerCode)
        {
            // Check which parameter is provided and query the database accordingly
            if (!string.IsNullOrEmpty(nationalId) && !string.IsNullOrEmpty(tandsCode))
            {
                return await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                    .Where(t => t.EMS_NATIONAL_ID == nationalId && t.TANDSCODE == tandsCode  )
                    .ToListAsync();
            }

            else
            {
                // If none of the parameters is provided, return an empty list or throw an exception, depending on your requirements
                return new List<TandSDetail>();
            }
        }


        public async Task<TandSDetail> GetTandSDetailById(int id)
        {
            // Assuming you have access to a database context named dbContext
            var detail = await _context.TRAVELLING_AND_SUBSISTENCE_DETAILS
                                         .FirstOrDefaultAsync(d => d.Id == id);
            return detail;
        }


        public async Task<TandSAdvance> GetTandSAdvance(string nationalId, string tandsCode, string subKey, string examinerCode)
        {
            // Check which parameter is provided and query the database accordingly
            if (!string.IsNullOrEmpty(nationalId) && !string.IsNullOrEmpty(tandsCode) && !string.IsNullOrEmpty(subKey) && !string.IsNullOrEmpty(examinerCode))
            {
                var detail = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES
                                       .FirstOrDefaultAsync(d => d.EMS_NATIONAL_ID == nationalId && d.TANDSCODE == tandsCode);
                return detail;
            }
     
            else
            {
                // If none of the parameters is provided, return an empty list or throw an exception, depending on your requirements
                return null;
            }
        }

        public async Task UpdateTandSAdvance(TandSAdvance tands, string userid)
        {
            var existingEntity = await _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == tands.EMS_NATIONAL_ID && e.TANDSCODE == tands.TANDSCODE);
            if (existingEntity != null)
            {

                existingEntity.ADJ_ADV_ACCOMMODATION_NONRES = tands.ADJ_ADV_ACCOMMODATION_NONRES;
                existingEntity.ADJ_ADV_ACCOMMODATION_RES = tands.ADJ_ADV_ACCOMMODATION_RES;
                existingEntity.ADJ_ADV_BREAKFAST = tands.ADJ_ADV_BREAKFAST;
                existingEntity.ADJ_ADV_DINNER = tands.ADJ_ADV_DINNER;
                existingEntity.ADJ_ADV_LUNCH = tands.ADJ_ADV_LUNCH;
                existingEntity.ADJ_ADV_TEAS = tands.ADJ_ADV_TEAS;
                existingEntity.ADJ_ADV_OVERNIGHTALLOWANCE = tands.ADJ_ADV_OVERNIGHTALLOWANCE;
                existingEntity.ADJ_ADV_TRANSPORT = tands.ADJ_ADV_TRANSPORT;
                existingEntity.ADJ_ADV_TOTAL = tands.ADJ_ADV_TOTAL;


                _context.TRAVELLING_AND_SUBSISTENCE_ADVANCES.Update(existingEntity);
                await _context.SaveChangesAsync(userid);
            }



        }

    }



}
