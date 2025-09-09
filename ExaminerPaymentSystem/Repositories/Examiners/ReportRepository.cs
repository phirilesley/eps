using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Linq;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context; // Your database context
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ReportDataResult> GetFilteredData(string filterStatus, string venue, string activity, string examCode, string subject, string paperCode, string regionCode,ApplicationUser applicationUser)
        {

            var userRoles = await _userManager.GetRolesAsync(applicationUser);

           


                var query = _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                .Include(a => a.Examiner)
                .Include(a => a.Examiner.ExaminerScriptsMarkeds)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
                .Include(t => t.TandSFiles)
                .AsQueryable();

            // Apply filters (excluding filterStatus for now)
            if (!string.IsNullOrEmpty(venue))
            {
                query = query.Where(t => t.EMS_VENUE == venue);
            }

            if (!string.IsNullOrEmpty(activity))
            {
                query = query.Where(t => t.EMS_PURPOSEOFJOURNEY == activity);
            }

            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode));
            }

            if (!string.IsNullOrEmpty(subject))
            {
                var sub = subject.Substring(3);
                query = query.Where(t => t.EMS_SUBKEY.Substring(3, 4) == sub);
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.Substring(7, 2) == paperCode);
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                // Step 1: Get entries from the register that match the region code
                var regionInRegister = await _context.EXAMINER_TRANSACTIONS
                    .Where(r => r.EMS_MARKING_REG_CODE == regionCode)
                    .ToListAsync();

                // Extract EMS_NATIONAL_ID and EMS_SUBKEY from the filtered transactions
                var regionIds = regionInRegister
                    .Select(r => (r.EMS_NATIONAL_ID, r.EMS_SUBKEY)) // Store as tuple
                    .ToList();

                // Step 2: Get T&S data for the filtered transactions
                var tandSData = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                    .Where(t => regionIds.Any(r => r.EMS_NATIONAL_ID == t.EMS_NATIONAL_ID && r.EMS_SUBKEY == t.EMS_SUBKEY))
                    .ToListAsync();

                // Update the query with the filtered TandS data
                query = tandSData.AsQueryable();
            }

            // Handle filterStatus
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "All")
                {
                    // No additional filtering needed
                }
                else if (filterStatus == "Pending"){
                    if (userRoles != null && userRoles.Contains("SubjectManager"))
                    {
                        query = query.Where(t => t.SUBJECT_MANAGER_STATUS == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                    {
                        query = query.Where(t => t.CENTRE_SUPERVISOR_STATUS == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("Accounts"))
                    {
                        query = query.Where(t => t.ACCOUNTS_STATUS == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("PeerReviewer"))
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == "Pending");
                    }
                   
                }
                else if (filterStatus == "Approved")
                {
                

                    if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                    {
                        query = query.Where(t => t.CENTRE_SUPERVISOR_STATUS == "Approved");
                    }

                    if (userRoles != null && userRoles.Contains("Accounts"))
                    {
                        query = query.Where(t => t.ACCOUNTS_STATUS == "Approved");
                    }

                    if (userRoles != null && userRoles.Contains("PeerReviewer"))
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == "Approved");
                    }
                }
                else if (filterStatus == "Recommended")
                {
                    query = query.Where(t => t.SUBJECT_MANAGER_STATUS == "Recommended");
                }
                else if (filterStatus == "Absent")
                {
                    // Get T&S data
                    var filteredTandSData = await query.ToListAsync();
                    var tandSIds = filteredTandSData
                        .Select(t => (t.EMS_NATIONAL_ID, t.EMS_SUBKEY)) // Store as tuple
                        .ToList();

                    // Step 2: Get entries from the register that are marked as Absent
                    var absentInRegister = await _context.EXAMINER_TRANSACTIONS
                        .Include(a => a.Examiner)
                        .Where(r => r.RegisterStatus == "Absent")
                        .ToListAsync();

                    // Step 3: Find Absent entries that are also present in the filtered T&S data
                    var absentWithTandS = absentInRegister
                        .Where(r => tandSIds.Any(t => t.EMS_NATIONAL_ID == r.EMS_NATIONAL_ID && t.EMS_SUBKEY == r.EMS_SUBKEY))
                        .ToList();

                    var selectedAbsentEntries = absentWithTandS
       .Select(r => new TandS
       {
           EMS_SUBKEY = r.EMS_SUBKEY,
           EMS_EXAMINER_CODE = r.EMS_EXAMINER_CODE,
           EMS_NATIONAL_ID = r.EMS_NATIONAL_ID,
           Examiner = r.Examiner,
         

       })
       .ToList();

                    query = selectedAbsentEntries.AsQueryable();
                }
                else if (filterStatus == "Missing")
                {
                    // Get T&S data
                    var filteredTandSData = await query.ToListAsync();
                    var tandSIds = filteredTandSData
                        .Select(t => (t.EMS_NATIONAL_ID, t.EMS_SUBKEY))
                        .ToList();

                    // Get entries from the register that are marked as Present
                    var presentInRegister = await _context.EXAMINER_TRANSACTIONS
    .Include(r => r.Examiner) // Eager load the Examiner
    .Where(r => r.RegisterStatus == "Present")
    .ToListAsync();

                    // Find entries that are in the register but not in the filtered T&S data
                    var missingEntries = presentInRegister
                        .Where(r => !tandSIds.Any(t => t.EMS_NATIONAL_ID == r.EMS_NATIONAL_ID && t.EMS_SUBKEY == r.EMS_SUBKEY))
                        .ToList();


                    var selectedMissingEntries = missingEntries
    .Select(r => new TandS
    {
        EMS_SUBKEY = r.EMS_SUBKEY,
        EMS_EXAMINER_CODE = r.EMS_EXAMINER_CODE,
        EMS_NATIONAL_ID = r.EMS_NATIONAL_ID,
        Examiner = r.Examiner,
        STATUS = "No T and S"
       
    })
    .ToList();


                    query = selectedMissingEntries.AsQueryable();
                }
            }

            // Get the total number of records (before pagination)
            var totalRecords = query.Count();


            // Map the results to ReportData
            var report = new List<ReportData>();
            foreach (var t in query)
            {
                var status = t.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == t.EMS_SUBKEY);
                if (status != null)
                {
                    var newReport = new ReportData()
                    {
                        LastName = t.Examiner.EMS_LAST_NAME,
                        FirstName = t.Examiner.EMS_EXAMINER_NAME,
                        IdNumber = t.EMS_NATIONAL_ID,
                        Subject = t.EMS_SUBKEY.Substring(3, 4) + "/" + t.EMS_SUBKEY.Substring(7, 2),
                        Phone = t.Examiner.EMS_PHONE_HOME,
                        RegisterStatus = status.RegisterStatus ?? "Not Available",
                        Status = t.STATUS == "No T and S" ? "No T and S" : "Available T and S"
                    };
                    report.Add(newReport);
                }
            }

            // Return the result
            return new ReportDataResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };
        }


        public async Task<ReportAccountsDataResult> GetFilteredAccountsData(string filterStatus, string venue, string activity, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser)
        {

            var userRoles = await _userManager.GetRolesAsync(applicationUser);




            var query = _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
            .Include(a => a.Examiner)
            .Include(a => a.Examiner.ExaminerScriptsMarkeds)
            .Include(t => t.TandSDetails)
            .Include(t => t.TandSAdvance)
            .Include(t => t.TandSFiles)
            .AsQueryable();

            // Apply filters (excluding filterStatus for now)
            if (!string.IsNullOrEmpty(venue))
            {
                query = query.Where(t => t.EMS_VENUE == venue);
            }

            if (!string.IsNullOrEmpty(activity))
            {
                query = query.Where(t => t.EMS_PURPOSEOFJOURNEY == activity);
            }

            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode));
            }

            if (!string.IsNullOrEmpty(subject))
            {
                var sub = subject.Substring(3);
                query = query.Where(t => t.EMS_SUBKEY.Substring(3, 4) == sub);
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.Substring(7, 2) == paperCode);
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                // Step 1: Get entries from the register that match the region code
                var regionInRegister = await _context.EXAMINER_TRANSACTIONS
                    .Where(r => r.EMS_MARKING_REG_CODE == regionCode)
                    .ToListAsync();

                // Extract EMS_NATIONAL_ID and EMS_SUBKEY from the filtered transactions
                var regionIds = regionInRegister
                    .Select(r => (r.EMS_NATIONAL_ID, r.EMS_SUBKEY)) // Store as tuple
                    .ToList();

                // Step 2: Get T&S data for the filtered transactions
                var tandSData = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                    .Where(t => regionIds.Any(r => r.EMS_NATIONAL_ID == t.EMS_NATIONAL_ID && r.EMS_SUBKEY == t.EMS_SUBKEY))
                    .ToListAsync();

                // Update the query with the filtered TandS data
                query = tandSData.AsQueryable();
            }

            // Handle filterStatus
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "All")
                {
                    // No additional filtering needed
                }
                else if (filterStatus == "Pending")
                {
              

                    if (userRoles != null && userRoles.Contains("Accounts"))
                    {
                        query = query.Where(t => t.ACCOUNTS_STATUS == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("PeerReviewer"))
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("AssistantAccountant"))
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == "Pending" || t.ACCOUNTS_STATUS == "Pending");
                    }

                }
                else if (filterStatus == "Approved")
                {

                    if (userRoles != null && userRoles.Contains("Accounts"))
                    {
                        query = query.Where(t => t.ACCOUNTS_STATUS == "Approved");
                    }

                    if (userRoles != null && userRoles.Contains("PeerReviewer"))
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == "Approved");
                    }

                    if (userRoles != null && userRoles.Contains("AssistantAccountant"))
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == "Approved" && t.ACCOUNTS_STATUS == "Approved");
                    }
                }
            
                else if (filterStatus == "Paid")
                {
                    query = query.Where(t => t.PaidStatus == "Paid");
                }
                else if (filterStatus == "Not Paid")
                {
                    query = query.Where(t => t.PaidStatus== "NotPaid");
                }
            }

            // Get the total number of records (before pagination)
            var totalRecords = query.Count();


            // Map the results to ReportData
            var report = new List<ReportAccountsData>();
            foreach (var t in query)
            {
                var status = t.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == t.EMS_SUBKEY);
                if (status != null)
                {
                   var finalDays = t.TandSAdvance.ADJ_ADV_DINNER.GetValueOrDefault();

                    var newReport = new ReportAccountsData()
                    {
                        LastName = t.Examiner.EMS_LAST_NAME,
                        FirstName = t.Examiner.EMS_EXAMINER_NAME,
                        IdNumber = t.EMS_NATIONAL_ID,
                        Subject = t.EMS_SUBKEY.Substring(3, 4) + "/" + t.EMS_SUBKEY.Substring(7, 2),
                        Phone = t.Examiner.EMS_PHONE_HOME,
                        RegisterStatus = status.RegisterStatus,
                        Days = finalDays.ToString(),
                        Amount = t.ADJ_TOTAL.GetValueOrDefault().ToString(),
                        Balance = (t.ADJ_TOTAL.GetValueOrDefault() - t.PaidAmount.GetValueOrDefault()).ToString(),
                        Paid = t.PaidAmount.GetValueOrDefault().ToString(),
                        PaidStatus =t.PaidStatus,
                        Status  = t.ACCOUNTS_REVIEW,

                    };

                    
                    report.Add(newReport);
                }
            }

            // Return the result
            return new ReportAccountsDataResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };
        }

        public async Task<ReportAccountsEPSDataResult> GetFilteredAccountsEPSData(string filterStatus, string venue, string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser)
        {

            var userRoles = await _userManager.GetRolesAsync(applicationUser);




            var query = _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_ACTIVITY == "BEM")
            .Include(a => a.Examiner)
            .AsQueryable();

            // Apply filters (excluding filterStatus for now)
            if (!string.IsNullOrEmpty(venue))
            {
                query = query.Where(t => t.EMS_VENUE == venue);
            }

    

            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode));
            }

            if (!string.IsNullOrEmpty(subject))
            {
                var sub = subject.Substring(3);
                query = query.Where(t => t.EMS_SUBKEY.Substring(3, 4) == sub);
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.Substring(7, 2) == paperCode);
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                query = query.Where(t => t.EMS_MARKING_REG_CODE == regionCode);
            }

            // Handle filterStatus
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "All")
                {
                    // No additional filtering needed
                }
                else if (filterStatus == "Pending")
                {


             
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending");
                   

                }
                else if (filterStatus == "Approved")
                {

                 
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved");
                  
                }

                else if (filterStatus == "Paid")
                {
                    query = query.Where(t => t.PaidStatus == "Paid");
                }
                else if (filterStatus == "NotPaid")
                {
                    query = query.Where(t => t.PaidStatus == "NotPaid");
                }
            }

            // Get the total number of records (before pagination)
            var totalRecords = query.Count();


            // Map the results to ReportData
            var report = new List<ReportAccountsEPSData>();
            foreach (var t in query)
            {

                    var newReport = new ReportAccountsEPSData()
                    {
                        LastName = t.Examiner.EMS_LAST_NAME,
                        FirstName = t.Examiner.EMS_EXAMINER_NAME,
                        IdNumber = t.EMS_NATIONAL_ID,
                        Subject = t.EMS_SUB_SUB_ID.Substring(3, 4) + "/" + t.EMS_PAPER_CODE,
                        Phone = t.Examiner.EMS_PHONE_HOME,
                        RegisterStatus = t.RegisterStatus,
                        Responsibility = t.RESPONSIBILITY_FEES.GetValueOrDefault().ToString(),
                        Coordination = t.COORDINATION_FEES.GetValueOrDefault().ToString(),
                        Capturing = t.CAPTURING_FEES.GetValueOrDefault().ToString(),
                        GrandTotal = t.GRAND_TOTAL.GetValueOrDefault().ToString(),
                        Category = t.EMS_ECT_EXAMINER_CAT_CODE,
                        ScriptsMarked = t.SCRIPTS_MARKED.GetValueOrDefault(),
                        Amount = (t.GRAND_TOTAL.GetValueOrDefault() + t.CAPTURING_FEES.GetValueOrDefault()).ToString(),
                        Balance = (t.GRAND_TOTAL.GetValueOrDefault() + t.CAPTURING_FEES.GetValueOrDefault() - t.PaidAmount.GetValueOrDefault()).ToString(),
                        Paid = t.PaidAmount.GetValueOrDefault().ToString(),
                        PaidStatus = t.PaidStatus,
                        Status = t.EMS_CENTRE_SUPERVISOR_STATUS,
                    };


                    report.Add(newReport);
                
            }

            // Return the result
            return new ReportAccountsEPSDataResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };
        }



        public async Task<ReportScriptsDataResult> GetScriptsFilteredData(string filterStatus, string examCode, string subject, string paperCode, string regionCode,ApplicationUser applicationUser)
        {
            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            var query = _context.EXAMINER_TRANSACTIONS
                .Include(a => a.Examiner)
                .Where(a => a.EMS_ACTIVITY == "BEM")
                .AsQueryable();

       
            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode));
            }

            if (!string.IsNullOrEmpty(subject))
            {
              
                query = query.Where(t => t.EMS_SUB_SUB_ID == subject);
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_PAPER_CODE == paperCode);
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                query = query.Where(t => t.EMS_MARKING_REG_CODE == regionCode);

            }

            // Handle filterStatus
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "All")
                {
                    // No additional filtering needed
                }
               
                else if (filterStatus == "Missing")
                {
                    query = query.Where(t => t.SCRIPTS_MARKED <= 0 && t.RegisterStatus == "Present" && t.IsPresent);
                }
                else if (filterStatus == "Pending")
                {
                    if (userRoles != null && userRoles.Contains("SubjectManager"))
                    {
                        query = query.Where(t => t.EMS_CERTIFIED_STATUS == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending");
                    }

                    if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS"))
                    {
                        query = query.Where(t => t.EMS_APPROVED_STATUS == "Pending");
                    }

                }
                else if (filterStatus == "Approved")
                {
                    if (userRoles != null && userRoles.Contains("SubjectManager"))
                    {
                        query = query.Where(t => t.EMS_CERTIFIED_STATUS == "Certified");
                    }

                    if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved");
                    }

                    if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("DPMS") || userRoles.Contains("RPMS"))
                    {
                        query = query.Where(t => t.EMS_APPROVED_STATUS == "Approved");
                    }
                }
             
            }

            // Get the total number of records (before pagination)
            var totalRecords = query.Count();


            // Map the results to ReportData
            var report = new List<ReportScriptsData>();
            foreach (var t in query)
            {
                var status = t.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == t.EMS_SUBKEY);
                if (status != null)
                {
                    var newReport = new ReportScriptsData()
                    {
                        LastName = t.Examiner.EMS_LAST_NAME,
                        FirstName = t.Examiner.EMS_EXAMINER_NAME,
                        IdNumber = t.EMS_NATIONAL_ID,
                        Subject = t.EMS_SUBKEY.Substring(3, 4) + "/" + t.EMS_SUBKEY.Substring(7, 2),
                         Supervisor = t.EMS_EXM_SUPERORD,
                         ScriptsRegister = true ? "Present" : "Absent",
                        RegisterStatus = t.RegisterStatus,
                        ScriptsMarked = t.SCRIPTS_MARKED.GetValueOrDefault(),
                    };
                    report.Add(newReport);
                }
            }

            // Return the result
            return new ReportScriptsDataResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };
        }


        public async Task<ReportRegisterDataResult> GetRegisterFilteredData(string filterStatus,string activity ,string examCode, string subject, string paperCode, string regionCode, ApplicationUser applicationUser)
        {
            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            var query = _context.EXAMINER_TRANSACTIONS
                .Include(a => a.Examiner)
                .AsQueryable();


            if (!string.IsNullOrEmpty(activity))
            {
                query = query.Where(t => t.EMS_ACTIVITY == activity);

            }

            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode));
            }

            if (!string.IsNullOrEmpty(subject))
            {

                query = query.Where(t => t.EMS_SUB_SUB_ID == subject);
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_PAPER_CODE == paperCode);
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                query = query.Where(t => t.EMS_MARKING_REG_CODE == regionCode);

            }

            // Handle filterStatus
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "All")
                {
                    // No additional filtering needed
                }
                else if (filterStatus == "Present")
                {
                    query = query.Where(t => t.RegisterStatus == "Present");
                }
                else if (filterStatus == "Absent")
                {
                    query = query.Where(t => t.RegisterStatus == "Absent");
                }
                else if (filterStatus == "Yes")
                {
                    query = query.Where(t => t.AttendanceStatus == "Yes");
                }
                else if (filterStatus == "No")
                {
                    query = query.Where(t => t.AttendanceStatus == "No");

                }else if(filterStatus == "DidNotMark")
                {
                    query = query.Where(t => t.IsPresent == false && t.RegisterStatus == "Present");
                }

            }

            // Get the total number of records (before pagination)
            var totalRecords = query.Count();


            // Map the results to ReportData
            var report = new List<ReportRegisterData>();
            foreach (var t in query)
            {
                var status = t.Examiner.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_SUBKEY == t.EMS_SUBKEY);
                if (status != null)
                {
                    var newReport = new ReportRegisterData()
                    {
                        LastName = t.Examiner.EMS_LAST_NAME,
                        FirstName = t.Examiner.EMS_EXAMINER_NAME,
                        IdNumber = t.EMS_NATIONAL_ID,
                        Subject = t.EMS_SUBKEY.Substring(3, 4) + "/" + t.EMS_SUBKEY.Substring(7, 2),
                        Supervisor = t.EMS_EXM_SUPERORD,
                       ExaminerNumber = t.EMS_EXAMINER_NUMBER,
                        RegisterStatus = t.RegisterStatus,
                        Role = t.EMS_ECT_EXAMINER_CAT_CODE,
                        Attendance = t.AttendanceStatus
                        
                    };
                    report.Add(newReport);
                }
            }

            // Return the result
            return new ReportRegisterDataResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };
        }


        //[HttpPost]
        //public async Task<IActionResult> GetApprovalStatusReport([FromBody] DataTablesRequest request)
        //{
            

        //    // Access filter values
        //    var filterStatus = request.FilterStatus;
        //    var examCode = request.ExamCode;
        //    var subject = request.Subject;
        //    var paperCode = request.PaperCode;

        //    // Get data from repositories
        //    var transactionData = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.Where(a => a.);
     

        //    // Combine and process the data
        //    var reportData = ProcessReportData(checklistData, transactionData, tandSData, filterStatus);

        //    return Json(new
        //    {
        //        draw = request.Draw,
        //        recordsTotal = reportData.Count,
        //        recordsFiltered = reportData.Count,
        //        data = reportData
        //    });
        //}



    }

    public class ReportDataResult
    {
        public int TotalRecords { get; set; } // Total records in the database
        public int FilteredRecords { get; set; } // Number of records after filtering
        public List<ReportData> Results { get; set; } // Filtered data
    }

    public class ReportAccountsDataResult
    {
        public int TotalRecords { get; set; } // Total records in the database
        public int FilteredRecords { get; set; } // Number of records after filtering
        public List<ReportAccountsData> Results { get; set; } // Filtered data
    }

    public class ReportAccountsEPSDataResult
    {
        public int TotalRecords { get; set; } // Total records in the database
        public int FilteredRecords { get; set; } // Number of records after filtering
        public List<ReportAccountsEPSData> Results { get; set; } // Filtered data
    }

    public class ReportScriptsDataResult
    {
        public int TotalRecords { get; set; } // Total records in the database
        public int FilteredRecords { get; set; } // Number of records after filtering
        public List<ReportScriptsData> Results { get; set; } // Filtered data
    }

    public class ReportRegisterDataResult
    {
        public int TotalRecords { get; set; } // Total records in the database
        public int FilteredRecords { get; set; } // Number of records after filtering
        public List<ReportRegisterData> Results { get; set; } // Filtered data
    }

    public class ReportData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string IdNumber { get; set; }
        public string Subject { get; set; }
        public string Phone { get; set; }
        public string RegisterStatus { get; set; }
        public string Status { get; set; }
    }

    public class ReportAccountsData
    {
        public string SubKey { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string IdNumber { get; set; }
        public string Subject { get; set; }

        public string Venue { get; set; }
        public string Phone { get; set; }
        public string RegisterStatus { get; set; }
        public string Days { get; set; }
        public string Balance { get; set; }
        public string Paid { get; set; }
        public string Amount { get; set; }
        public string PaidStatus { get; set; }
        public string Status { get; set; }
    }

    public class ReportAccountsEPSData
    {
        public string SubKey { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string IdNumber { get; set; }
        public string Subject { get; set; }
        public string Phone { get; set; }
        public string RegisterStatus { get; set; }
        public string Category { get; set; }
        public int ScriptsMarked { get; set; }
        public string Responsibility { get; set; }

        public string Coordination { get; set; }

        public string Capturing { get; set; }

        public string GrandTotal { get; set; }


        public string Balance { get; set; }
        public string Paid { get; set; }
        public string Amount { get; set; }
        public string PaidStatus { get; set; }
        public string Status { get; set; }
    }

    public class ReportScriptsData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string IdNumber { get; set; }
        public string Subject { get; set; }
        public string Supervisor { get; set; }
        public string RegisterStatus { get; set; }
        public string ScriptsRegister { get; set; }

        public int ScriptsMarked { get; set; }
    }

    public class ReportRegisterData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string IdNumber { get; set; }
        public string Subject { get; set; }
        public string Supervisor { get; set; }
        public string RegisterStatus { get; set; }
        public string Attendance { get; set; }

        public string ExaminerNumber { get; set; }

        public string Role { get; set; }
    }
}
