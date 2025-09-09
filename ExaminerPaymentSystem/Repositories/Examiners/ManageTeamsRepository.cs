using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Controllers.Examiners;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System.ComponentModel;
using System.Diagnostics;
using static QuestPDF.Helpers.Colors;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class ManageTeamsRepository : IManageTeamsRepository
    {
       
        private readonly ApplicationDbContext _context;
        public ManageTeamsRepository(ApplicationDbContext context)
        {
              _context = context;
        }
        public async Task<IEnumerable<ExaminerTeam>> GetComponentTeamsAsync(string examCode, string subject, string paperCode, string regionCode)
        {
            // Mock data for demonstration
            return await Task.FromResult(new List<ExaminerTeam>
        {
            new ExaminerTeam
            { SubKey = "1",
                BMSCode = "1001",
                SupervisorName = "John Doe",
                ExaminerNumber = "1001",
                Role = "PMS",
                District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",
                MarkingRegion = "01",
                
                TeamMembers = new List<Team>
                {
                    new Team { SubKey = "2",ExaminerName = "Jane Smith", ExaminerNumber = "2001",Role = "DPMS", BMSCode = "1001",  District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",MarkingRegion = "01"},
                    new Team { SubKey = "3",ExaminerName = "Mike Johnson", ExaminerNumber = "2002", Role= "DPMS",BMSCode = "1001",District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",MarkingRegion = "01" }
                }
            },
            new ExaminerTeam
            {SubKey = "2",
                BMSCode = "1001",
                SupervisorName = "Jane Smith",
                ExaminerNumber = "2001",
                Role = "DPMS",
                District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",
                MarkingRegion = "01",
                TeamMembers = new List<Team>()
                {
                     new Team { ExaminerName = "Tom June", ExaminerNumber = "3001",Role = "BMS", BMSCode = "2001",District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",MarkingRegion = "01"},
                    new Team {ExaminerName = "Sam April", ExaminerNumber = "3002", Role = "BMS", BMSCode = "2001", District = "Harare", Phone = "07777717717", CapturingRole = "", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare", MarkingRegion = "01"}
                }
            },
             new ExaminerTeam
            {SubKey = "3",
                BMSCode = "1001",
                SupervisorName = "Mike Johnson",
                ExaminerNumber = "2002",
                Role = "DPMS",
                District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",
                MarkingRegion = "01",
                TeamMembers = new List<Team>()
                {
                     new Team {SubKey = "4",ExaminerName = "Law Smithy", ExaminerNumber = "3003", Role = "BMS", BMSCode = "2002", District = "Harare", Phone = "07777717717", CapturingRole = "", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare", MarkingRegion = "01"},
                    new Team {SubKey = "5", ExaminerName = "Micheal Holy", ExaminerNumber = "3004", Role = "BMS", BMSCode = "2002", District = "Harare", Phone = "07777717717", CapturingRole = "", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare",MarkingRegion = "01"}
                }
            },
              new ExaminerTeam
            {SubKey = "4",
                BMSCode = "2001",
                SupervisorName = "Tom June",
                ExaminerNumber = "3001",
                Role = "BMS",
                District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",
                MarkingRegion = "01",
                TeamMembers = new List<Team>()
                {
                     new Team {SubKey = "6",ExaminerName = "Devon Felis", ExaminerNumber = "4001", Role = "E", BMSCode = "3001", District = "Harare", Phone = "07777717717", CapturingRole = "C", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare",MarkingRegion = "01"},
                    new Team {SubKey = "7",ExaminerName = "Anna Moyo", ExaminerNumber = "4002", Role = "E", BMSCode = "3001", District = "Harare", Phone = "07777717717", CapturingRole = "", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare", MarkingRegion = "01"}
                }
            },
              new ExaminerTeam
            {SubKey = "5",
                BMSCode = "2001",
                SupervisorName = "Sam April",
                ExaminerNumber = "3002",
                Role = "BMS",
                District = "Harare",
                Phone = "07777717717",
                CapturingRole = "",
                IdNumber = "445445555J45",
                Sex = "M",
                Station = "Allan Wison High",
                Province ="Harare",
                MarkingRegion = "01",
                TeamMembers = new List<Team>()
                {
                     new Team {SubKey = "8",ExaminerName = "Rex Ncube", ExaminerNumber = "4003", Role = "E", BMSCode = "3002", District = "Harare", Phone = "07777717717", CapturingRole = "V", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare", MarkingRegion = "01"},
                    new Team {SubKey = "9",ExaminerName = "Holy Ten", ExaminerNumber = "4004", Role = "E", BMSCode = "3002", District = "Harare", Phone = "07777717717", CapturingRole = "", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare",MarkingRegion = "01"},
                    new Team {SubKey = "10",ExaminerName = "Holy Ten", ExaminerNumber = "4005", Role = "E", BMSCode = "3002", District = "Harare", Phone = "07777717717", CapturingRole = "", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare", MarkingRegion = "01"},
                    new Team {SubKey = "11",ExaminerName = "Holy Ten", ExaminerNumber = "4006", Role = "E", BMSCode = "3002", District = "Harare", Phone = "07777717717", CapturingRole = "C", IdNumber = "445445555J45", Sex = "M", Station = "Allan Wison High", Province = "Harare",MarkingRegion = "01"}
                }
            }
        });
        }

        public async Task<IEnumerable<SelectTeamViewModel>> GetTeamsFromMasterAsync(string examCode, string subject, string paperCode, string regionCode,string activity)
        {

            try
            {
                List<SelectTeamViewModel>  teamview = new List<SelectTeamViewModel>();

    //            var examinersInTransaction = await _context.EXAMINER_TRANSACTIONS
    //.Where(a => a.EMS_SUB_SUB_ID == (examCode + subject) && a.EMS_PAPER_CODE == paperCode)
    //.Include(a => a.Examiner)
    //.ToListAsync();

    //            // Extract non-null Examiners from transactions
    //            var transactionExaminers = examinersInTransaction
    //                .Where(a => a.Examiner != null)
    //                .Select(a => a.Examiner)
    //                .ToList();

    //            var allComponentExaminers = await _context.EXM_EXAMINER_MASTER
    //                .Where(a => a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == paperCode)
    //                .Include(a => a.ExaminerScriptsMarkeds)
    //                .ToListAsync();

    //            // Combine and deduplicate lists
    //            var combinedExaminers = allComponentExaminers
    //                .Union(transactionExaminers)
    //                .ToList();

                // Get all examiners involved in transactions
                var examinersInTransaction = await _context.EXAMINER_TRANSACTIONS
                    .Where(a => a.EMS_SUB_SUB_ID == examCode + subject && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity)
                    .Include(a => a.Examiner)
                    .Include(a => a.Examiner.ExaminerScriptsMarkeds)
                    .Select(a => a.Examiner)
                    .Where(e => e != null)
                    .ToListAsync();

                // Get all examiners from EXM_EXAMINER_MASTER
                var allComponentExaminers = await _context.EXM_EXAMINER_MASTER
                    .Where(a => a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == paperCode)
                    .Include(a => a.ExaminerScriptsMarkeds)
                    .ToListAsync();

                // Combine both lists, avoiding duplicates based on unique identifier (e.g., ExaminerCode)
                var combined = allComponentExaminers
                    .Concat(examinersInTransaction)
                    .DistinctBy(a => a.EMS_NATIONAL_ID)
                    .ToList();



                if (!string.IsNullOrEmpty(regionCode))
                {
                    combined = combined
                        .Where(a => a.EMS_MARKING_REG_CODE == regionCode)
                        .ToList();
                }
                foreach (var component in combined)
                {
                    string station = string.IsNullOrEmpty(component.EMS_WORK_ADD1)
    ? string.Empty
    : component.EMS_WORK_ADD1.Substring(0, Math.Min(15, component.EMS_WORK_ADD1.Length));
                    var subkey = examCode + subject + paperCode + activity + component.EMS_NATIONAL_ID;
                    var transaction = component.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == component.EMS_NATIONAL_ID && a.EMS_SUBKEY == subkey && a.EMS_ACTIVITY == activity && a.EMS_SUB_SUB_ID == examCode + subject && a.EMS_PAPER_CODE == paperCode);

                    if(transaction != null)
                    {
                        var data = new SelectTeamViewModel
                        {
                            Name = component.EMS_EXAMINER_NAME + " " + component.EMS_LAST_NAME,
                            IdNumber = transaction.EMS_NATIONAL_ID,
                            Sex = component.EMS_SEX,
                            Category = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                            CapturingRole = string.IsNullOrWhiteSpace(transaction.EMS_CAPTURINGROLE) ? "N/A" : transaction.EMS_CAPTURINGROLE,
                            Station = station,
                            Province = component.EMS_WORK_ADD3,
                            District = component.EMS_WORK_ADD2,
                            ExaminerNumber = transaction.EMS_EXAMINER_NUMBER,
                            Team = transaction.EMS_EXM_SUPERORD,
                            Selected = "Selected",
                            Status = "Selected",
                            RegisterStatus = transaction.RegisterStatus
                        };

                        teamview.Add(data);

                    }
                    else
                    {
                        var data = new SelectTeamViewModel
                        {
                            Name = component.EMS_EXAMINER_NAME + " " + component.EMS_LAST_NAME,
                            IdNumber = component.EMS_NATIONAL_ID,
                            Sex = component.EMS_SEX,
                            Category = component.EMS_ECT_EXAMINER_CAT_CODE,
                            CapturingRole = "N/A",
                            Station = station,
                            Province = component.EMS_WORK_ADD3,
                            District = component.EMS_WORK_ADD2,
                            ExaminerNumber = "",
                            Team = "",
                            Selected = "Pending",
                            Status = "Pending",
                            
                        };

                        teamview.Add(data);
                    }

                }

             

                return teamview;

            }
            catch (Exception)
            {

                throw;
            }

        }


        public async Task<IEnumerable<Examiner>> GetFromMasterAsync(string examCode, string subject, string paperCode, string regionCode, string activity)
        {

            try
            {
               
                var allComponentExaminers = await _context.EXM_EXAMINER_MASTER
      .Where(a => a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == paperCode)
      .Include(a => a.ExaminerScriptsMarkeds)
      .ToListAsync();

                if (!string.IsNullOrEmpty(regionCode))
                {
                    allComponentExaminers = allComponentExaminers
                        .Where(a => a.EMS_MARKING_REG_CODE == regionCode)
                        .ToList();
                }




                return allComponentExaminers;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<SelectTeamViewModel>> GetAllFromMasterAsync()
        {
            try
            {
                var allComponentExaminers = await _context.EXM_EXAMINER_MASTER
                    .Where(a => !a.EMS_EXAMINER_CODE.StartsWith("0"))
                    .Include(a => a.ExaminerScriptsMarkeds)
                    .ToListAsync();

                var data = allComponentExaminers.Select(component => new SelectTeamViewModel
                {
                    Name = component.EMS_EXAMINER_NAME + " " + component.EMS_LAST_NAME,
                    IdNumber = component.EMS_NATIONAL_ID,
                    Sex = component.EMS_SEX,
                    Category = component.EMS_ECT_EXAMINER_CAT_CODE,
                    Region = component.EMS_MARKING_REG_CODE,
                    CapturingRole = "N/A",
                    SubjectCode = component.EMS_SUB_SUB_ID,
                    PaperCode = component.EMS_PAPER_CODE,
                    Province = component.EMS_WORK_ADD3,
                    District = component.EMS_WORK_ADD2,
                    ExaminerNumber = component.EMS_EXAMINER_NUMBER,
                    Team = component.EMS_EXM_SUPERORD,
                    Selected = "Pending",
                    Status = "Pending",
                    Subjects = component.ExaminerScriptsMarkeds
        .Select(s => s.EMS_SUB_SUB_ID + "/" + s.EMS_PAPER_CODE + "/" + s.EMS_ACTIVITY)
        .Distinct()
        .ToList()
                });


                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public async Task<IEnumerable<ExaminerScriptsMarked>> GetSelectedTeamsFromTransactionAsync(string examCode, string subject, string paperCode, string regionCode,string activity)
        {

            try
            {
                var allComponentExaminers = await _context.EXAMINER_TRANSACTIONS
      .Where(a => a.EMS_SUB_SUB_ID == examCode +subject && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity)
      .Include(a => a.Examiner)
      .ToListAsync();

                if (!string.IsNullOrEmpty(regionCode))
                {
                    allComponentExaminers = allComponentExaminers
                        .Where(a => a.EMS_MARKING_REG_CODE == regionCode)
                        .ToList();
                }

                return allComponentExaminers;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<Apportioned>> GetApportionedScriptsAsync(string examCode, string subject, string paperCode, string regionCode, string activity)
        {

            try
            {
                List<Apportioned> apport = new List<Apportioned> ();

                if (!string.IsNullOrEmpty(regionCode))
                {
                   apport = await _context.REF_CAT_PAPER
      .Where(a => a.CTP_PPR_SUB_PAPER_CODE== examCode + subject + paperCode && a.CTP_REGION_CODE == regionCode)
      .ToListAsync();



                }
                else
                {
                    apport = await _context.REF_CAT_PAPER
.Where(a => a.CTP_PPR_SUB_PAPER_CODE == examCode + subject + paperCode)
.ToListAsync();

                }

                return apport;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<ExaminerApportionment>> GetApportionedExaminersAsync(string examCode, string subject, string paperCode, string regionCode, string activity)
        {

            try
            {
                List<ExaminerApportionment> apport = new List<ExaminerApportionment>();

                if (!string.IsNullOrEmpty(regionCode))
                {
                    apport = await _context.ExaminerApportionment
       .Where(a => a.sub_sub_id== examCode + subject && a.PaperCode == paperCode && a.RegionCode == regionCode)
       .ToListAsync();



                }
                else
                {
                    apport = await _context.ExaminerApportionment
      .Where(a => a.sub_sub_id == examCode + subject && a.PaperCode == paperCode)
      .ToListAsync();

                }

                return apport;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<OperationResult> UpdateTransactionRecord(ExaminerUpdateModel model, ApplicationUser currentUser)
        {
            var transactionKey = $"{model.ExamCode}{model.SubjectCode}";

            var existing = await _context.EXAMINER_TRANSACTIONS
                .FirstOrDefaultAsync(a =>
                    a.EMS_SUB_SUB_ID == transactionKey &&
                    a.EMS_PAPER_CODE == model.PaperCode &&
                    a.EMS_ACTIVITY == model.Activity &&
                    a.EMS_NATIONAL_ID == model.IdNumber);

            if (model.IsSelected)
            {
                if (existing == null)
                {
                    var examiner = await _context.EXM_EXAMINER_MASTER
                        .FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == model.IdNumber);

                    if (examiner == null)
                    {
                        return new OperationResult
                        {
                            Success = false,
                            Message = "Examiner not found"
                        };
                    }

                    var newTransaction = new ExaminerScriptsMarked
                    {
                        EMS_NATIONAL_ID = model.IdNumber,
                        EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                        EMS_ECT_EXAMINER_CAT_CODE = model.Category,
                        EMS_CAPTURINGROLE = model.CapturingRole,
                        EMS_EXAMINER_NUMBER = model.ExaminerNumber,
                        EMS_EXM_SUPERORD = model.Team,
                        EMS_SUB_SUB_ID = transactionKey,
                        EMS_PAPER_CODE = model.PaperCode,
                        EMS_ACTIVITY = model.Activity,
                        EMS_SUBKEY = $"{transactionKey}{model.PaperCode}{model.Activity}{model.IdNumber}",
                        EMS_MARKING_REG_CODE = model.RegionCode ?? examiner.EMS_MARKING_REG_CODE,
                        IsPresent = false,
                        RegisterStatus = "Absent",
                        RegisterStatusBy = currentUser.UserName,
                        RegisterStatusDate = DateTime.Now.ToString(),
                        RecommendedStatus = "Pending",
                        RecommendedBy = currentUser.UserName,
                        RecommendedDate = DateTime.Now.ToString(),
                        AttendanceStatus = "Pending",
                        AttendanceStatusBy = currentUser.UserName,
                        AttendanceStatusDate = DateTime.Now.ToString(),
                        SCRIPTS_MARKED = 0
                    };

                    await _context.EXAMINER_TRANSACTIONS.AddAsync(newTransaction);
                    await _context.SaveChangesAsync(currentUser.Id);


                    return new OperationResult
                    {
                        Success = true,
                        Message = "Examiner Selected"
                    };
                }
                
            }
            else
            {
                if (existing != null)
                {
                    _context.EXAMINER_TRANSACTIONS.Remove(existing);
                    await _context.SaveChangesAsync(currentUser.Id);
              

                    return new OperationResult
                    {
                        Success = true,
                        Message = "Examiner Removed"
                    };
                }
            }

            return new OperationResult
            {
                Success = true,
                Message = "Success"
            };
        }

        //   public async Task<OperationResult> UpdateTransactionRecord(ExaminerUpdateModel updatedExaminerData,ApplicationUser currentUser)
        //   {
        //       string message = "";
        //      if(updatedExaminerData.IsSelected)
        //       {
        //           var existing = await _context.EXAMINER_TRANSACTIONS
        //.FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == (updatedExaminerData.ExamCode + updatedExaminerData.SubjectCode) && a.EMS_PAPER_CODE == updatedExaminerData.PaperCode && a.EMS_ACTIVITY == updatedExaminerData.Activity && a.EMS_NATIONAL_ID == updatedExaminerData.IdNumber);

        //           if (existing == null)
        //           {
        //               var examiner = await _context.EXM_EXAMINER_MASTER.FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == updatedExaminerData.IdNumber);

        //               if (examiner == null)
        //               {
        //                   return new OperationResult
        //                   {
        //                       Success = false,
        //                       Message = "Examiner not found"
        //                   };
        //               }

        //               var subkey = updatedExaminerData.ExamCode + updatedExaminerData.SubjectCode + updatedExaminerData.PaperCode + updatedExaminerData.Activity + updatedExaminerData.IdNumber;
        //               var newTransction = new ExaminerScriptsMarked()
        //               {

        //                   EMS_NATIONAL_ID = updatedExaminerData.IdNumber,
        //                   EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
        //                   EMS_ECT_EXAMINER_CAT_CODE = updatedExaminerData.Category,
        //                   EMS_CAPTURINGROLE = updatedExaminerData.CapturingRole,
        //                   EMS_EXAMINER_NUMBER = updatedExaminerData.ExaminerNumber,
        //                   EMS_EXM_SUPERORD = updatedExaminerData.Team,
        //                   EMS_SUB_SUB_ID = updatedExaminerData.ExamCode + updatedExaminerData.SubjectCode,
        //                   EMS_PAPER_CODE = updatedExaminerData.PaperCode,
        //                   EMS_ACTIVITY = updatedExaminerData.Activity,
        //                   EMS_SUBKEY = subkey,
        //                   EMS_MARKING_REG_CODE = !string.IsNullOrWhiteSpace(updatedExaminerData.RegionCode)
        //                   ? updatedExaminerData.RegionCode
        //                   : examiner.EMS_MARKING_REG_CODE,
        //                   IsPresent = false,
        //                   RegisterStatus = "Absent",
        //                   RegisterStatusBy = currentUser.UserName,
        //                   RegisterStatusDate = DateTime.Now.ToString(),
        //                   RecommendedStatus = "Pending",
        //                   RecommendedBy = currentUser.UserName,
        //                   RecommendedDate = DateTime.Now.ToString(),
        //                   AttendanceStatus = "Pending",
        //                   AttendanceStatusBy = currentUser.UserName,
        //                   AttendanceStatusDate = DateTime.Now.ToString(),
        //                   SCRIPTS_MARKED = 0
        //               };

        //             await _context.EXAMINER_TRANSACTIONS.AddAsync(newTransction);




        //               message = "Examiner Selected";
        //           }

        //       }
        //       else
        //       {
        //           var existing = await _context.EXAMINER_TRANSACTIONS
        //.FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == (updatedExaminerData.ExamCode + updatedExaminerData.SubjectCode) && a.EMS_PAPER_CODE == updatedExaminerData.PaperCode && a.EMS_ACTIVITY == updatedExaminerData.Activity && a.EMS_NATIONAL_ID == updatedExaminerData.IdNumber);
        //           _context.EXAMINER_TRANSACTIONS.Remove(existing);

        //           message = "Examiner Removed";
        //       }

        //       await _context.SaveChangesAsync(currentUser.Id);
        //       return new OperationResult
        //       {
        //           Success = true,
        //           Message = message
        //       };
        //   }

        public async Task<OperationResult> UpdateCapturingRole(string idNumber, string capturingRole, string examCode, string subjectCode, string paperCode, string regionCode, string activity, string userId)
        {
            try
            {
                var existing = await _context.EXAMINER_TRANSACTIONS
     .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_NATIONAL_ID == idNumber);

                if (existing != null)          
             { 
                existing.EMS_CAPTURINGROLE = capturingRole;
                     _context.EXAMINER_TRANSACTIONS.Update(existing);

                    await _context.SaveChangesAsync(userId);
                }
               
                return new OperationResult
                {
                    Success = true,
                    Message = "Updated"
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<OperationResult> UpdateCategory(string idNumber, string category, string examCode, string subjectCode, string paperCode, string regionCode, string activity, string userId)
        {   
            if(category == "C" || category == "V")
            {
              var result =  await UpdateCapturingRole(idNumber,category,examCode,subjectCode,paperCode,regionCode,activity,userId);

                if (result.Success)
                {
                    return new OperationResult
                    {
                        Success = true,
                        Message = "Capturing Role Updated"
                    };
                }
                if (!result.Success)
                {
                    return new OperationResult
                    {
                        Success = true,
                        Message = "Capturing Role Failed"
                    };
                }
            }    

            try
            {
                if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                {
                    var existing = await _context.EXAMINER_TRANSACTIONS
                        .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == examCode + subjectCode &&
                                                  a.EMS_PAPER_CODE == paperCode &&
                                                  a.EMS_ACTIVITY == activity &&
                                                  a.EMS_NATIONAL_ID == idNumber && a.EMS_MARKING_REG_CODE == regionCode);

                    if (existing != null)
                    {
                   

                        var pmsList = await _context.EXAMINER_TRANSACTIONS
                            .Where(a => a.EMS_ACTIVITY == activity &&
                                        a.EMS_SUB_SUB_ID == examCode + subjectCode &&
                                        a.EMS_PAPER_CODE == paperCode &&
                                        a.EMS_ECT_EXAMINER_CAT_CODE == category && a.EMS_MARKING_REG_CODE == regionCode)
                            .ToListAsync();

                        if (!int.TryParse(regionCode, out var regionNum))
                        {
                            return new OperationResult { Success = false, Message = "Invalid region code." };
                        }
                        string prefix = regionNum.ToString(); // "1".."10"

                        // Find next running number within this context
                        int nextNumber = pmsList
                            .Select(a =>
                            {
                                var numStr = a.EMS_EXAMINER_NUMBER;
                                if (string.IsNullOrWhiteSpace(numStr)) return 0;
                                if (!numStr.StartsWith(prefix)) return 0;

                                var suffix = numStr.Substring(prefix.Length); // the 3-digit part
                                return int.TryParse(suffix, out var n) ? n : 0;
                            })
                            .DefaultIfEmpty(0)
                            .Max() + 1;

                        // Build new examiner number: prefix + 3-digit suffix
                        string newExaminerNumber = $"{prefix}{nextNumber:D3}";
                        // Examples: prefix="1" -> 1001, 1002 ... ; prefix="10" -> 10001, 10002 ...

                        existing.EMS_EXAMINER_NUMBER = newExaminerNumber;
                        existing.EMS_ECT_EXAMINER_CAT_CODE = category;

                        _context.EXAMINER_TRANSACTIONS.Update(existing);
                        await _context.SaveChangesAsync(userId);
                    }
                }

                else
                {
                    var existing = await _context.EXAMINER_TRANSACTIONS
   .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_NATIONAL_ID == idNumber);

                    if (existing != null)
                    {

                        // Define category prefixes (first digit only)
                        var categoryPrefix = category switch
                        {
                            "PMS" => '1',
                            "RPMS" => '2',
                            "DPMS" => '2',
                            "BMS" => '3',
                            "E" => '4',
                            _ => throw new InvalidOperationException("Invalid category")
                        };

                        var pmsList = await _context.EXAMINER_TRANSACTIONS
                            .Where(a => a.EMS_ACTIVITY == activity &&
                                        a.EMS_SUB_SUB_ID == examCode + subjectCode &&
                                        a.EMS_PAPER_CODE == paperCode &&
                                        a.EMS_ECT_EXAMINER_CAT_CODE == category)
                            .ToListAsync();

                        // Extract valid numbers (must start with correct prefix and be parsable)
                        var validNumbers = new List<int>();
                        var invalidEntries = new List<string>(); // Track bad data for reporting

                        foreach (var item in pmsList)
                        {
                            if (item.EMS_EXAMINER_NUMBER?.StartsWith(categoryPrefix) == true &&
                                int.TryParse(item.EMS_EXAMINER_NUMBER, out int num))
                            {
                                validNumbers.Add(num);
                            }
                            else if (!string.IsNullOrEmpty(item.EMS_EXAMINER_NUMBER))
                            {
                                invalidEntries.Add(item.EMS_EXAMINER_NUMBER);
                            }
                        }

                        // Calculate next number (max + 1, or category's starting number if empty)
                        int nextNumber = validNumbers.Any()
                            ? validNumbers.Max() + 1
                            : int.Parse($"{categoryPrefix}001"); // PMS=1001, RPMS=2001, etc.

                        // Log invalid entries (optional)
                     
                        //if (!nextNumber.ToString().StartsWith(prefix.ToString()))
                        //{
                        //    return new OperationResult
                        //    {
                        //        Success = false,
                        //        Message = $"Invalid generated examiner number check if there is a {category} with number in this {nextNumber} sequence. The number must start with {prefix}."
                        //    };
                        //}

                        // Convert to string for storage
                        string newExaminerNumber = nextNumber.ToString();

                        existing.EMS_EXAMINER_NUMBER = newExaminerNumber;
                        existing.EMS_ECT_EXAMINER_CAT_CODE = category;
                        _context.EXAMINER_TRANSACTIONS.Update(existing);

                        await _context.SaveChangesAsync(userId);

                        if (invalidEntries.Any())
                        {
                            return new OperationResult
                            {
                                Success = true,
                                Message = $"Updated category but Found {invalidEntries.Count} examiner numbers with incorrect prefixes: {string.Join(", ", invalidEntries)}"

                            };
                            // Alternatively: notify user via return value or event
                        }
                    }
                }



                return new OperationResult
                {
                    Success = true,
                    Message = "Updated"
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<OperationResult> UpdateExaminerNumber(string idNumber, string examinerNumber, string examCode, string subjectCode, string paperCode, string regionCode, string activity,string userId)
        {
            try
            {
                var existing = await _context.EXAMINER_TRANSACTIONS
     .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_NATIONAL_ID == idNumber);

                if (existing != null)
                {
                    existing.EMS_EXAMINER_NUMBER = examinerNumber;
                    _context.EXAMINER_TRANSACTIONS.Update(existing);

                    await _context.SaveChangesAsync(userId);
                }

                return new OperationResult
                {
                    Success = true,
                    Message = "Updated"
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<OperationResult> UpdateTeam(string idNumber, string team, string examCode, string subjectCode, string paperCode, string regionCode, string activity,string userId)
        {
            try
            {
                var existing = await _context.EXAMINER_TRANSACTIONS
     .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_NATIONAL_ID == idNumber);

                if (existing != null)
                {
                    existing.EMS_EXM_SUPERORD = team;
                    _context.EXAMINER_TRANSACTIONS.Update(existing);

                    await _context.SaveChangesAsync(userId);
                }

                return new OperationResult
                {
                    Success = true,
                    Message = "Updated"
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<string>> GetSuperordsBySubSubIdAndPaperCodeAsync(string examCode, string subSubId, string paperCode, string activity, string regionCode)
        {
            var result = new List<string>();
            if (!string.IsNullOrEmpty(regionCode) && subSubId.StartsWith("7"))
            {



                var validSuperords = new[] { "BMS", "PMS", "RPMS", "DPMS" };

                result = await _context.EXAMINER_TRANSACTIONS
                .Where(a => a.EMS_SUB_SUB_ID == examCode + subSubId &&
                            a.EMS_PAPER_CODE == paperCode &&
                            a.EMS_ACTIVITY == activity &&
                            a.EMS_MARKING_REG_CODE == regionCode &&
                            validSuperords.Contains(a.EMS_ECT_EXAMINER_CAT_CODE))  // Cleaner solution
                .Select(eem => eem.EMS_EXAMINER_NUMBER)
                .Distinct()
                .ToListAsync();




            }
            else
            {
                var validSuperords = new[] { "BMS", "PMS", "RPMS", "DPMS" };

                result = await _context.EXAMINER_TRANSACTIONS
                .Where(a => a.EMS_SUB_SUB_ID == examCode + subSubId &&
                            a.EMS_PAPER_CODE == paperCode &&
                            a.EMS_ACTIVITY == activity  &&
                            validSuperords.Contains(a.EMS_ECT_EXAMINER_CAT_CODE))  // Cleaner solution
                .Select(eem => eem.EMS_EXAMINER_NUMBER)
                .Distinct()
                .ToListAsync();


             }

            return result;
        }


        public async Task<MarksCaptured> GetComponentMarkCaptured(string examCode, string subjectCode, string paperCode, string regionCode)
        {
            try
            {
                var scripData = new MarksCaptured();
                if (!string.IsNullOrEmpty(regionCode) && !string.IsNullOrEmpty(subjectCode))
                {
                    scripData = await _context.EXM_SCRIPT_CAPTURED.FirstOrDefaultAsync(e => e.ExamCode == examCode && e.SubjectCode == subjectCode && e.PaperCode == paperCode && e.RegionCode == regionCode);
                }
                else
                {
                    if (!string.IsNullOrEmpty(subjectCode))
                    {
                        scripData = await _context.EXM_SCRIPT_CAPTURED.FirstOrDefaultAsync(e => e.ExamCode == examCode && e.SubjectCode == subjectCode && e.PaperCode == paperCode);
                    }


                }
                return scripData;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<Apportioned>> GetApportionedScriptsAsync(string examCode, string subject, string paperCode, string regionCode)
        {
            var approtioned = await _context.REF_CAT_PAPER.Where( a => a.CTP_PPR_SUB_PAPER_CODE == examCode + subject + paperCode).ToListAsync();

            if (!string.IsNullOrEmpty(regionCode) && subject.StartsWith("7"))
            {
                approtioned = approtioned.Where(a => a.CTP_REGION_CODE == regionCode).ToList();
            }

            return approtioned;
            }

        public async Task<OperationResult> SaveApportionment(ApportionScriptsViewModel model, string examCode, string subjectCode, string paperCode, string regionCode, string activity,string userId)
        {
            try
            {
                string paperCodeKey = examCode + subjectCode + paperCode;

                // Define a list of roles and map them to model properties
                var roles = new List<(string Category, int Selected, int MaxScripts)>
        {
            ("PMS", model.SelectedPMS, model.MaxScriptsPMS),
            ("RPMS", model.SelectedRPMS, model.MaxScriptsRPMS),
            ("DPMS", model.SelectedDPMS, model.MaxScriptsDPMS),
            ("BMS", model.SelectedBMS, model.MaxScriptsBMS),
            ("E", model.SelectedE, model.MaxScriptsE)
        };

                foreach (var role in roles)
                {
                    if (role.MaxScripts > 0) // Only save if there is a MaxScripts value
                    {
                        Apportioned existingRecord = null;

                        if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                        {
                            existingRecord = await _context.REF_CAT_PAPER
                                .AsNoTracking() // Prevents tracking issues
                                .FirstOrDefaultAsync(a => a.CTP_PPR_SUB_PAPER_CODE == paperCodeKey &&
                                                          a.CTP_ECT_CAT_CODE == role.Category &&
                                                          a.CTP_REGION_CODE == regionCode);
                        }
                        else
                        {
                            existingRecord = await _context.REF_CAT_PAPER
                                .AsNoTracking() // Prevents tracking issues
                                .FirstOrDefaultAsync(a => a.CTP_PPR_SUB_PAPER_CODE == paperCodeKey &&
                                                          a.CTP_ECT_CAT_CODE == role.Category);
                        }

                        if (existingRecord != null)
                        {
                            // Update existing record
                            existingRecord.CTP_MAX_SCRIPTS = role.MaxScripts;
                            _context.REF_CAT_PAPER.Update(existingRecord);
                        }
                        else
                        {
                            // Insert new record
                            var newRecord = new Apportioned
                            {
                                CTP_PPR_SUB_PAPER_CODE = paperCodeKey,
                                CTP_ECT_CAT_CODE = role.Category,
                                CTP_MAX_SCRIPTS = role.MaxScripts,
                                CTP_REGION_CODE = regionCode
                            };

                            await _context.REF_CAT_PAPER.AddAsync(newRecord);
                        }
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync(userId);

                return new OperationResult
                {
                    Success = true,
                    Message = "Apportionment saved successfully"
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Error saving apportionment: " + ex.Message
                };
            }
        }


        public async Task<OperationResult> SaveExaminerApportionment(ExaminerApportionmentViewModel model, string examCode, string subjectCode, string paperCode, string regionCode, string activity, string userId)
        {
            try
            {
                string paperCodeKey = examCode + subjectCode;

                // Define a list of roles and map them to model properties
                var roles = new List<(string Category, int Selected,int Scripts,int SharePerExaminer)>
        {
            ("PMS", model.TotalPMS,model.SharePMS,model.ScriptsToExaminers),
            ("RPMS", model.TotalRPMS,model.ShareRPMS,model.ScriptsToExaminers),
            ("DPMS", model.TotalDPMS,model.ShareDPMS,model.ScriptsToExaminers),
            ("BMS", model.TotalBMS, model.ShareBMS, model.ScriptsToExaminers),
            ("E", model.TotalE, model.ShareE, model.ScriptsToExaminers)
        };

                foreach (var role in roles)
                {
                    if (role.Selected > 0) // Only save if there is a MaxScripts value
                    {
                        ExaminerApportionment existingRecord = null;

                        if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                        {
                            existingRecord = await _context.ExaminerApportionment
                                .AsNoTracking() // Prevents tracking issues
                                .FirstOrDefaultAsync(a => a.sub_sub_id == paperCodeKey && a.PaperCode == paperCode &&
                                                          a.category == role.Category &&
                                                          a.RegionCode == regionCode);
                        }
                        else
                        {
                            existingRecord = await _context.ExaminerApportionment
                                .AsNoTracking() // Prevents tracking issues
                                .FirstOrDefaultAsync(a => a.sub_sub_id == paperCodeKey && a.PaperCode == paperCode &&
                                                          a.category == role.Category);
                        }

                        if (existingRecord != null)
                        {
                            // Update existing record
                            existingRecord.TotalExaminers = role.Selected;
                            existingRecord.ScriptAToExaminerX = role.SharePerExaminer;
                            existingRecord.ScriptPerExaminer = role.Scripts;
                            _context.ExaminerApportionment.Update(existingRecord);
                        }
                        else
                        {
                            // Insert new record
                            var newRecord = new ExaminerApportionment
                            {
                                sub_sub_id = paperCodeKey,
                                PaperCode = paperCode,
                                category = role.Category,
                                TotalExaminers = role.Selected,
                                ScriptAToExaminerX = role.SharePerExaminer,
                                ScriptPerExaminer = role.Scripts,
                                RegionCode = regionCode
                            };

                            await _context.ExaminerApportionment.AddAsync(newRecord);
                        }
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync(userId);

                return new OperationResult
                {
                    Success = true,
                    Message = "Apportionment saved successfully"
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Error saving apportionment: " + ex.Message
                };
            }
        }



        public async Task<OperationResult> SaveSummaryScriptApportionment(SummaryScriptApportionmentViewModel model, string examCode, string subjectCode, string paperCode, string regionCode, string activity)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> ProcessTeamUpdates(List<TeamMemberImportModel> teamList ,string examCode, string subject, string paperCode, string regionCode, string activity, ApplicationUser currentUser)
        {
            try
            {
                teamList = teamList
    .GroupBy(x => examCode + subject + paperCode + activity + x.IdNumber)
    .Select(g => g.First())
    .ToList();
                foreach (var item in teamList)
                {
                    var existing = await _context.EXAMINER_TRANSACTIONS
         .FirstOrDefaultAsync(a => a.EMS_SUB_SUB_ID == examCode + subject && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_NATIONAL_ID == item.IdNumber);


                    if (existing == null)
                    {
                        var examiner = await _context.EXM_EXAMINER_MASTER.FirstOrDefaultAsync(a => a.EMS_NATIONAL_ID == item.IdNumber);

                        if (examiner != null)
                        {
                            var subkey = examCode + subject + paperCode + activity + item.IdNumber;
                            var newTransction = new ExaminerScriptsMarked()
                            {

                                EMS_NATIONAL_ID = item.IdNumber,
                                EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                                EMS_ECT_EXAMINER_CAT_CODE = item.Status,
                                EMS_CAPTURINGROLE = item.Capturing,
                                EMS_EXAMINER_NUMBER = item.ExaminerNumber,
                                EMS_EXM_SUPERORD = item.Team,
                                EMS_SUB_SUB_ID = examCode + subject,
                                EMS_PAPER_CODE = paperCode,
                                EMS_ACTIVITY = activity,
                                EMS_SUBKEY = subkey,
                                EMS_MARKING_REG_CODE = !string.IsNullOrWhiteSpace(item.Region)
                             ? item.Region
                             : examiner.EMS_MARKING_REG_CODE,
                                IsPresent = false,
                                RegisterStatus = "Absent",
                                RegisterStatusBy = currentUser.UserName,
                                RegisterStatusDate = DateTime.Now.ToString(),
                                RecommendedStatus = "Pending",
                                RecommendedBy = currentUser.UserName,
                                RecommendedDate = DateTime.Now.ToString(),
                                AttendanceStatus = "Pending",
                                AttendanceStatusBy = currentUser.UserName,
                                AttendanceStatusDate = DateTime.Now.ToString(),
                                SCRIPTS_MARKED = 0
                            };

                                _context.EXAMINER_TRANSACTIONS.Add(newTransction);

                            // Save changes to the database
                            await _context.SaveChangesAsync(currentUser.Id);
                        }
                        else
                        {
                            existing.EMS_EXAMINER_NUMBER = item.ExaminerNumber;
                            existing.EMS_EXM_SUPERORD = item.Team;
                            existing.EMS_MARKING_REG_CODE = item.Region;
                            existing.EMS_ECT_EXAMINER_CAT_CODE = item.Status;
                            existing.EMS_CAPTURINGROLE = item.Capturing;

                            _context.EXAMINER_TRANSACTIONS.Update(existing);
                            // Save changes to the database
                            await _context.SaveChangesAsync(currentUser.Id);
                        }



                    }


                }

      

                return new OperationResult
                {
                    Success = true,
                    Message = "Selected examiners successfully saved"
                };



            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Error saving apportionment: " + ex.Message
                };
            }



        }

        public async Task<OperationResult> ClearTeam(string examCode, string subjectCode, string paperCode, string regionCode, string activity, string userId)
        {
            var existingData = new List<ExaminerScriptsMarked>();
            var existingUserData = new List<ApplicationUser>();

            if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
            {

                existingData = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_MARKING_REG_CODE == regionCode).ToListAsync();

               foreach (var item in existingData)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(a => a.IDNumber == item.EMS_NATIONAL_ID && a.EMS_SUBKEY == item.EMS_SUBKEY);
                    existingUserData.Add(user);
                }
            }
            else
            {
                existingData = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity).ToListAsync();
                foreach (var item in existingData)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(a => a.IDNumber == item.EMS_NATIONAL_ID && a.EMS_SUBKEY == item.EMS_SUBKEY);
                 
                    if (user != null)
                    {
                        existingUserData.Add(user);
                    }

                }

            }
            if (existingData.Any())
            {
                _context.EXAMINER_TRANSACTIONS.RemoveRange(existingData);
                await _context.SaveChangesAsync(userId);

                await    RemoveUsers(existingUserData,userId);
           
            }

            return new OperationResult
            {
                Success = true,
                Message = "Selected examiners successfully saved"
            };
        }

        public async Task<bool> RemoveUsers(List<ApplicationUser> existingUserData, string userId)
        {
            try
            {
                if (existingUserData.Any())
                {
                    foreach (var user in existingUserData)
                    {
                        // Remove any roles for this user first
                        var roles = _context.UserRoles.Where(ur => ur.UserId == user.Id);
                        _context.UserRoles.RemoveRange(roles);
                    }

                    _context.Users.RemoveRange(existingUserData);
                }

                await _context.SaveChangesAsync(userId); // Ensure this is your custom SaveChangesAsync
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
