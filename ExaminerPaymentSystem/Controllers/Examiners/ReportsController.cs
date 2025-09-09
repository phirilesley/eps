using CsvHelper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2013.Excel;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Extensions;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using ExaminerPaymentSystem.Repositories.Common;
using ExaminerPaymentSystem.Repositories.Examiners;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ExaminerPaymentSystem.Controllers.Examiners
{
    public class ReportsController : Controller
    {

        private readonly IExaminerRepository _examinerRepository;
        private readonly ITandSRepository _tandSRepository;
        private readonly ITandSDetailsRepository _detailRepository;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IAdvanceFeesRepository _advanceFeesRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IRateAndTaxRepository _taxRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRegisterRepository _registerRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ITranscribersRepository _transcribersRepository;
        private readonly IReportRepository _reportRepository;


        public ReportsController(IExaminerRepository examinerRepository, SignInManager<ApplicationUser> signInManager, ITandSRepository tandSRepository, ITandSDetailsRepository tandSDetailsRepository, ApplicationDbContext applicationDbContext, IAdvanceFeesRepository advanceFeesRepository, ITransactionRepository transactionRepository, IRateAndTaxRepository taxRepository, UserManager<ApplicationUser> userManager, IRegisterRepository registerRepository, IVenueRepository venueRepository,ITranscribersRepository transcribersRepository, IReportRepository reportRepository)
        {
            _examinerRepository = examinerRepository;
            _tandSRepository = tandSRepository;
            _detailRepository = tandSDetailsRepository;
            _signInManager = signInManager;
            _context = applicationDbContext;
            _advanceFeesRepository = advanceFeesRepository;
            _transactionRepository = transactionRepository;
            _taxRepository = taxRepository;
            _userManager = userManager;
            _registerRepository = registerRepository;
            _venueRepository = venueRepository;
            _transcribersRepository = transcribersRepository;
            _reportRepository = reportRepository;
        }


        [Authorize]
        public IActionResult TravellingAndSubstance()
        {

            
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DownloadAdvanceComponentCSV(string examCode = "", string subject = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "")
        {
         

            // Generate the Excel file
            string filePath = await ClaculateAdvances(examCode, subject, paperCode, venue,activity,regionCode);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";



            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"tands_report_{subject + "/" + paperCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [Authorize]
        public async Task<IActionResult> DownloadAdvanceVenueCSV(string examCode = "", string subject = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "", List<string> year = null)
        {
           

            // Generate the Excel file
            string filePath = await ClaculateAdvances(examCode,subject, paperCode, venue,activity,regionCode,year);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"tands_report_{venue}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [Authorize]
        public async Task<IActionResult> DownloadBanksExcelFile(string examCode = "", string subject = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "")
        {

            // Generate the Excel file
            string filePath = await getBanksExcell(subject, paperCode, examCode, venue, activity, regionCode);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"banks_report_{subject + "/" + paperCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }


        [Authorize]
        public async Task<IActionResult> MasterReportTandS(string activity = "", List<string> year = null)
        {

            string filePath = await ClaculateAdvances("", "", "", "", activity, "", year);



            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var fileName = $"Master_tands_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"; // Generates a filename like "data_20240908_123456.xlsx"
            return File(fileBytes, contentType, fileName);

        }


        private async Task<string> ClaculateAdvances(string examCode = "", string subject = "", string paperCode = "", string venue = "", string activity = "", string regionCode = "", List<string> year = null)
        {

            string excelFilePath = "data.xlsx";

            List<TravelAdvanceReport> accountsReports = new List<TravelAdvanceReport>();

            if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(paperCode) &&
                  !string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(activity))
            {


                var tandsByComponent = await _tandSRepository.GetTandSComponentReport(examCode, subject, paperCode, venue, activity, regionCode);
                tandsByComponent = tandsByComponent.Where(x => x.EMS_VENUE == venue && x.EMS_PURPOSEOFJOURNEY == activity && x.ACCOUNTS_REVIEW == "Approved" && x.ACCOUNTS_STATUS == "Approved").ToList();
                accountsReports = await MakeCulculationForAdvances(tandsByComponent);


            }
            else if (string.IsNullOrEmpty(examCode) && string.IsNullOrEmpty(subject) && string.IsNullOrEmpty(paperCode) &&
                !string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(activity))
            {
                var tandsByVenue = await _tandSRepository.GetVenueTandSReport(examCode, venue, activity, year);
                tandsByVenue = tandsByVenue.Where(x => x.EMS_VENUE == venue && x.EMS_PURPOSEOFJOURNEY == activity && x.ACCOUNTS_REVIEW == "Approved" && x.ACCOUNTS_STATUS == "Approved").ToList();


                accountsReports = await MakeCulculationForAdvances(tandsByVenue);

            }
            else
            {

                var masterTandS = await _tandSRepository.GetActivityTandSReport(examCode, activity, year);
                masterTandS = masterTandS.Where(x => x.ACCOUNTS_REVIEW == "Approved" && x.ACCOUNTS_STATUS == "Approved" && x.EMS_PURPOSEOFJOURNEY == activity).ToList();


                accountsReports = await MakeCulculationForAdvances(masterTandS);

            }

            CsvService csvService = new CsvService();
            await csvService.ExportToExcel(accountsReports, excelFilePath);

            return excelFilePath;

        }

        [Authorize]
        public async Task<string> ClaculateAdvancesTranscribers(string venue,string examCode,string activity)
        {
            string excelFilePath = "data.xlsx";

            List<TravelAdvanceReport> accountsReports = new List<TravelAdvanceReport>();

            if (!string.IsNullOrEmpty(venue) && !string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(activity))
            {
                var tandsByComponent = await _transcribersRepository.GetAllTandSForTranscribers(venue,examCode,activity);
                tandsByComponent = tandsByComponent.Where(x => x.ACCOUNTS_REVIEW == "Approved" && x.ACCOUNTS_STATUS == "Approved").ToList();
                accountsReports = await MakeCulculationForAdvances(tandsByComponent);
            }
            else
            {

                var masterTandS = await _transcribersRepository.GetFullListTandSForTranscribers();
                masterTandS = masterTandS.Where(x => x.ACCOUNTS_REVIEW == "Approved" && x.ACCOUNTS_STATUS == "Approved").ToList();


                accountsReports = await MakeCulculationForAdvances(masterTandS);

            }

            CsvService csvService = new CsvService();
            await csvService.ExportToExcel(accountsReports, excelFilePath);

            return excelFilePath;

        }


        private async Task<List<TravelAdvanceReport>> MakeCulculationForAdvances(IEnumerable<TandS> tandsByComponent)
        {
            var fees = await _advanceFeesRepository.GetFirstAdvanceFee();

            List<TravelAdvanceReport> accountsReports = new List<TravelAdvanceReport>();

            foreach (var item in tandsByComponent)
            {
                decimal transitbusfare = 0;
                decimal checkinnacc = 0;
                decimal checkinndiner = 0;
                decimal checkinnlunch = 0;

                foreach (var item1 in item.TandSDetails)
                {
                    transitbusfare += item1.ADJ_BUSFARE.GetValueOrDefault();
                    checkinnacc += item1.ADJ_ACCOMMODATION.GetValueOrDefault();
                    checkinndiner += item1.ADJ_DINNER.GetValueOrDefault();
                    checkinnlunch += item1.ADJ_LUNCH.GetValueOrDefault();

                }

                decimal finaldays = 0;


                decimal localbusfare = 0;
                decimal accomodation = 0;
                decimal breakfast = 0;
                decimal teas = 0;
                decimal lunch = 0;
                decimal dinner = 0;
                decimal supp = 0;


                decimal dinerandlunch = 0;


                if (item.TandSAdvance.ADJ_ADV_ACCOMMODATION_NONRES.GetValueOrDefault() != null)
                {
                    accomodation = item.TandSAdvance.ADJ_ADV_ACCOMMODATION_NONRES.GetValueOrDefault() * fees.FEE_ACCOMMODATION_NONRES.GetValueOrDefault();
                }
                else
                {
                    accomodation = item.TandSAdvance.ADJ_ADV_ACCOMMODATION_RES.GetValueOrDefault() * fees.FEE_ACCOMMODATION_RES.GetValueOrDefault();
                }

                finaldays = item.TandSAdvance.ADJ_ADV_DINNER.GetValueOrDefault();

                breakfast = item.TandSAdvance.ADJ_ADV_BREAKFAST.GetValueOrDefault() * fees.FEE_BREAKFAST.GetValueOrDefault();

                dinner = item.TandSAdvance.ADJ_ADV_DINNER.GetValueOrDefault() * fees.FEE_DINNER.GetValueOrDefault();

                lunch = item.TandSAdvance.ADJ_ADV_LUNCH.GetValueOrDefault() * fees.FEE_LUNCH.GetValueOrDefault();

                supp = item.TandSAdvance.ADJ_ADV_OVERNIGHTALLOWANCE.GetValueOrDefault() * fees.FEE_OVERNIGHTALLOWANCE.GetValueOrDefault();

                teas = item.TandSAdvance.ADV_TEAS.GetValueOrDefault() * fees.FEE_TEA.GetValueOrDefault();

                localbusfare = item.TandSAdvance.ADV_TRANSPORT.GetValueOrDefault() * fees.FEE_TRANSPORT.GetValueOrDefault();

                dinerandlunch = dinner + lunch;

                var total = transitbusfare + checkinnlunch + checkinnacc + checkinndiner + accomodation + localbusfare + breakfast + dinner + lunch + supp + teas;

                decimal lessAdv = 0;

                decimal USDBalance = total - lessAdv;
                var rateandtax = await _taxRepository.GetFirstTaxAndRate();
                decimal rate = rateandtax?.CurrentRate ?? 0m;
                decimal zigbalance = USDBalance * rate;
                decimal zigticket = 0;
                decimal zigpayment = zigbalance - zigticket;

                var tandsReport = new TravelAdvanceReport()
                {
                    TransitBusFare = transitbusfare.ToString("F2"),
                    TransitLunch = checkinnlunch.ToString("F2"),
                    CheckInnAccommodation = checkinnacc.ToString("F2"),
                    CheckInDinner = checkinndiner.ToString("F2"),
                    Days = finaldays.ToString("F0"),
                    Venue = item.EMS_VENUE,
                    LocalBusFare = localbusfare.ToString("F2"),
                    Accommodation = accomodation.ToString("F2"),
                    Breakfast = breakfast.ToString("F2"),
                    LunchAndDinner = dinerandlunch.ToString("F2"),
                    Supp = supp.ToString("F2"),
                    Total = total.ToString("F2"),
                    LessAdvance = lessAdv.ToString("F2"),
                    UsdBalance = USDBalance.ToString("F2"),
                    Rate = rate.ToString(),
                    ZigBalance = zigbalance.ToString("F2"),
                    ZigTicket = zigticket.ToString("F2"),
                    ZigPayment = zigpayment.ToString("F2"),
                    BankZIG = item.Examiner.EMS_BANK_NAME_ZWL,
                    BankBranchZIG = item.Examiner.EMS_BRANCH_CODE_ZWL,
                    BankAccountZIG = item.Examiner.EMS_ACCOUNT_NO_ZWL,
                    Name = item.Examiner.EMS_EXAMINER_NAME + " " + item.Examiner.EMS_LAST_NAME,
                    WorkAddress = item.Examiner.EMS_WORK_ADD1,
                    BankUSD = item.Examiner.EMS_BANK_NAME_FCA,
                    BankBranchUSD = item.Examiner.EMS_BRANCH_CODE_FCA,
                    BankAccountUSD = item.Examiner.EMS_ACCOUNT_NO_FCA,
                    Status = item.STATUS,
                    Subject = item.Examiner.EMS_SUB_SUB_ID + "/" + item.Examiner.EMS_PAPER_CODE


                };

                accountsReports.Add(tandsReport);
            }

            return accountsReports;
        }





        public async Task<string> getBanksExcell(string filterSubjectCode, string filterPaperCode, string filterExamCode, string venue, string activity, string regionCode)
        {
            var query = await _tandSRepository.GetTandSComponentReport(filterExamCode, filterSubjectCode, filterPaperCode, venue, activity, regionCode);


            query = query.Where(t => t.EMS_PURPOSEOFJOURNEY == activity && t.EMS_VENUE == venue);


            var reportData = query

                .Select(item => new MissingPeopleReportViewModel
                {

                    EMS_FullName = item.Examiner.EMS_EXAMINER_NAME + " " + item.Examiner.EMS_LAST_NAME,
                    Phone = item.Examiner.EMS_PHONE_HOME,
                    Bankzig = item.Examiner.EMS_ACCOUNT_NO_ZWL,
                    Bankfca = item.Examiner.EMS_ACCOUNT_NO_FCA,
                    BankNamefca = item.Examiner.EMS_BANK_NAME_FCA,
                    BankNamezig = item.Examiner.EMS_BANK_NAME_ZWL,
                    Branchfca = item.Examiner.EMS_BRANCH_CODE_FCA,
                    Branchzig = item.Examiner.EMS_BRANCH_CODE_ZWL,
                })
                .ToList();


            string excelFilePath = "data.xlsx";
            CsvService csvService = new CsvService();
            await csvService.ExportBanksToExcel(reportData, excelFilePath);

            return excelFilePath;
        }








        [Authorize]
        public IActionResult ExaminerPayments()
        {
           

            return View();
        }






        [Authorize]
        public async Task<IActionResult> GenerateComponentExcelFile(string examCode, string subject, string paperCode, string regionCode)
        {

            if(!string.IsNullOrEmpty(subject))
            {
                subject = subject.Substring(3);
            }
            // Generate the Excel file
            string filePath = await GenerateComponentExaminerAccountsReportExcel(examCode, subject, paperCode, regionCode);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"eps_report_{subject + "/" + paperCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }


        [Authorize]
        public async Task<IActionResult> GenerateLevelEPSExcelFile(string examCode)
        {


            // Generate the Excel file
            string filePath = await GenerateLevelExaminerAccountsReportExcel(examCode);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"eps_report_{examCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        [Authorize]
        public async Task<IActionResult> GenerateMasterEPSExcelFile(List<string> year)
        {


            // Generate the Excel file
            string filePath = await GenerateMasterExaminerAccountsReportExcel(year);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"master_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        private async Task<string> GenerateComponentExaminerAccountsReportExcel(string examCode, string subject, string paperCode, string regionCode)
        {
          
            string excelFilePath = "data.xlsx";

            var examinersfromtransction = await _transactionRepository.GetComponentExaminer(examCode, subject, paperCode, regionCode);

            List<TravelExaminerMarkingReport> accountsReports = await CalculateExaminerPayment(examinersfromtransction);

            CsvService csvService = new CsvService();
            await csvService.ExportToExcelExaminer(accountsReports, excelFilePath);

            return excelFilePath;
        }

        private async Task<string> GenerateLevelExaminerAccountsReportExcel(string examCode)
        {
           
            string excelFilePath = "data.xlsx";

            var examinersfromtransction = await _transactionRepository.GetLevelComponentExaminer(examCode);

            List<TravelExaminerMarkingReport> accountsReports = await CalculateExaminerPayment(examinersfromtransction);

            CsvService csvService = new CsvService();
            await csvService.ExportToExcelExaminer(accountsReports, excelFilePath);

            return excelFilePath;
        }

        private async Task<string> GenerateMasterExaminerAccountsReportExcel(List<string> examCode)
        {
           
            string excelFilePath = "data.xlsx";

            var examinersfromtransction = await _transactionRepository.GetMasterExaminers(examCode);

            List<TravelExaminerMarkingReport> accountsReports = await CalculateExaminerPayment(examinersfromtransction);

            CsvService csvService = new CsvService();
            await csvService.ExportToExcelExaminer(accountsReports, excelFilePath);

            return excelFilePath;
        }

        private async Task<List<TravelExaminerMarkingReport>> CalculateExaminerPayment(List<ExaminerScriptsMarked> examinersfromtransction)
        {
            List<TravelExaminerMarkingReport> markingReports = new List<TravelExaminerMarkingReport>();



            var rateandtax = await _taxRepository.GetFirstTaxAndRate();



            foreach (var item in examinersfromtransction)
            {
                if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Approved" && item.EMS_CERTIFIED_STATUS == "Certified" && item.EMS_CENTRE_SUPERVISOR_STATUS == "Approved")
                {


                    if (item.Examiner != null)
                    {

                        decimal totalAfterCupturing = 0;
                        decimal rate = 0;
                        decimal tax = 0;
                        decimal zig_amount = 0;
                        decimal amt_payable = 0;
                        decimal adjustedTax = 0;
                        decimal totalAfterScript = 0;

                        if (rateandtax != null)
                        {
                            rate = rateandtax.CurrentRate;
                            tax = rateandtax.WHT / 100;
                        }

                        totalAfterScript = item.SCRIPTS_MARKED.GetValueOrDefault() * item.SCRIPT_RATE.GetValueOrDefault();
                        totalAfterCupturing = item.GRAND_TOTAL.GetValueOrDefault() + item.CAPTURING_FEES.GetValueOrDefault();
                        zig_amount = totalAfterCupturing * rate;
                        if (totalAfterCupturing > 1000)
                        {
                            adjustedTax = zig_amount * tax;
                        }


                        var subjectCode1 = item.EMS_SUB_SUB_ID.Substring(3);
                        var paperCode1 = item.EMS_PAPER_CODE;
                        var subjectName = $"{subjectCode1}/{paperCode1}";

                        amt_payable = zig_amount - adjustedTax;


                        DateTime currentDate = DateTime.Now;


                        string currentDateAsString = currentDate.ToString("yyyy-MM-dd");

                        var report = new TravelExaminerMarkingReport()
                        {
                            Date = currentDateAsString,
                            BankName = item.Examiner.EMS_BANK_NAME_ZWL,
                            BankAccount = item.Examiner.EMS_ACCOUNT_NO_ZWL,
                            ShortCode = item.Examiner.EMS_BRANCH_CODE_ZWL,
                            Fullname = item.Examiner.EMS_EXAMINER_NAME + " " + item.Examiner.EMS_LAST_NAME,
                            Status = item.EMS_ECT_EXAMINER_CAT_CODE,
                            Subject = subjectName,
                            ScriptMarked = item.SCRIPTS_MARKED.ToString(),
                            ScriptRate = item.SCRIPT_RATE.ToString(),
                            TotalAfterScriptRate = totalAfterScript.ToString(),
                            Resp = item.RESPONSIBILITY_FEES.ToString(),
                            Coord = item.COORDINATION_FEES.ToString(),
                            GrandTotal = item.GRAND_TOTAL.ToString(),
                            Capturing = item.CAPTURING_FEES.ToString(),
                            Total = totalAfterCupturing.ToString(),
                            Rate = rate.ToString(),
                            ZIGAmount = zig_amount.ToString(),
                            WHT = adjustedTax.ToString(),
                            AmountPayable = amt_payable.ToString(),

                        };

                        markingReports.Add(report);

                    }


                }



            }

            return markingReports;
        }


        private async Task<List<TravelExaminerMarkingReport>> CalculateTranscribedScripts(List<ExaminerScriptsMarked> examinersfromtransction)
        {
            List<TravelExaminerMarkingReport> markingReports = new List<TravelExaminerMarkingReport>();



            var rateandtax = await _taxRepository.GetFirstTaxAndRate();



            foreach (var item in examinersfromtransction)
            {
                if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Approved" && item.EMS_CERTIFIED_STATUS == "Certified" && item.EMS_CENTRE_SUPERVISOR_STATUS == "Approved")
                {


                    if (item.Examiner != null)
                    {

                        decimal totalAfterCupturing = 0;
                        decimal rate = 0;
                        decimal tax = 0;
                        decimal zig_amount = 0;
                        decimal amt_payable = 0;
                        decimal adjustedTax = 0;
                        decimal totalAfterScript = 0;

                        if (rateandtax != null)
                        {
                            rate = rateandtax.CurrentRate;
                            tax = rateandtax.WHT / 100;
                        }

                        totalAfterScript = item.SCRIPTS_MARKED.GetValueOrDefault() * item.SCRIPT_RATE.GetValueOrDefault();
                        totalAfterCupturing = item.GRAND_TOTAL.GetValueOrDefault() + item.CAPTURING_FEES.GetValueOrDefault();
                        zig_amount = totalAfterCupturing * rate;
                        if (totalAfterCupturing > 1000)
                        {
                            adjustedTax = zig_amount * tax;
                        }


                        var subjectCode1 = item.EMS_SUB_SUB_ID.Substring(3);
                        var paperCode1 = item.EMS_PAPER_CODE;
                        var subjectName = $"{subjectCode1}/{paperCode1}";

                        amt_payable = zig_amount - adjustedTax;


                        DateTime currentDate = DateTime.Now;


                        string currentDateAsString = currentDate.ToString("yyyy-MM-dd");

                        var report = new TravelExaminerMarkingReport()
                        {
                            Date = currentDateAsString,
                            BankName = item.Examiner.EMS_BANK_NAME_ZWL,
                            BankAccount = item.Examiner.EMS_ACCOUNT_NO_ZWL,
                            ShortCode = item.Examiner.EMS_BRANCH_CODE_ZWL,
                            Fullname = item.Examiner.EMS_EXAMINER_NAME + " " + item.Examiner.EMS_LAST_NAME,
                            Status = item.EMS_ECT_EXAMINER_CAT_CODE,
                            Subject = subjectName,
                            ScriptMarked = item.SCRIPTS_MARKED.ToString(),
                            ScriptRate = item.SCRIPT_RATE.ToString(),
                            TotalAfterScriptRate = totalAfterScript.ToString(),
                            Resp = item.RESPONSIBILITY_FEES.ToString(),
                            Coord = item.COORDINATION_FEES.ToString(),
                            GrandTotal = item.GRAND_TOTAL.ToString(),
                            Capturing = item.CAPTURING_FEES.ToString(),
                            Total = totalAfterCupturing.ToString(),
                            Rate = rate.ToString(),
                            ZIGAmount = zig_amount.ToString(),
                            WHT = adjustedTax.ToString(),
                            AmountPayable = amt_payable.ToString(),

                        };

                        markingReports.Add(report);

                    }


                }



            }

            return markingReports;
        }



        [Authorize]
        public async Task<IActionResult> DownloadTranscriberVenueReport(string venue,string examCode,string activity)
        {
            var userSession = new SessionModel();
            if (!string.IsNullOrEmpty(venue))
            {
                userSession = new SessionModel()
                {
              
                    Venue = venue,
                    Activity = activity,
                    ExamCode = examCode

                };
                HttpContext.Session.SetObjectAsJson("Session", userSession);
            }
            else
            {
                userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            }

            // Generate the Excel file
            string filePath = await ClaculateAdvancesTranscribers(userSession.Venue,userSession.ExamCode,userSession.Activity);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"BT_tands_report_{venue}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [Authorize]
        public async Task<IActionResult> MasterTranscribersReportReportTandS(string venue= "")
        {
            

            // Generate the Excel file
            string filePath = await ClaculateAdvancesTranscribers("","","");

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"BT_tands_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }


        [Authorize]
        public IActionResult TranscriberScriptsReport()
        {


            return View();
        }


        [Authorize]
        public async Task<IActionResult> GenerateTranscribersExcelFile(string examCode)
        {


            // Generate the Excel file
            string filePath = await GenerateLevelExaminerAccountsReportExcel(examCode);

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"eps_report_{examCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        private async Task<string> GenerateTranscriberReportExcel(string examCode)
        {

            string excelFilePath = "data.xlsx";

            var examinersfromtransction  = await _context.EXAMINER_TRANSACTIONS
                  .Where(a => a.EMS_SUB_SUB_ID.StartsWith(examCode) && (a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" ))
                  .Include(a => a.Examiner)
              
                  .ToListAsync();

            List<TravelExaminerMarkingReport> accountsReports = await CalculateTranscribedScripts(examinersfromtransction);

            CsvService csvService = new CsvService();
            await csvService.ExportToExcelExaminer(accountsReports, excelFilePath);

            return excelFilePath;
        }




        [Authorize]
            public IActionResult Index(string examCode, string subject, string paperCode)
            {

                var userSession = new SessionModel();
                if (!string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(examCode) && !string.IsNullOrEmpty(examCode))
                {
                    userSession = new SessionModel()
                    {
                        ExamCode = examCode,
                        SubjectCode = subject.Substring(3),
                        PaperCode = paperCode

                    };
                    HttpContext.Session.SetObjectAsJson("Session", userSession);
                }
                else
                {
                    userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
                }
                // Assign values to ViewBag
                ViewBag.ExamCode = userSession.ExamCode;
                ViewBag.Subject = userSession.SubjectCode;
                ViewBag.PaperCode = userSession.PaperCode;

                return View();
            }






            [Authorize]
            public async Task<IActionResult> StatusReport(string filterExamCode, string filterSubjectCode, string filterPaperCode, string venue)
            {
                var query = await _tandSRepository.GetAllTandS();
                var register = await _registerRepository.GetAllRegister();
                var subKey = filterSubjectCode + filterPaperCode;
                var finalRegisterList = new List<RegisterDto>();

                if (!string.IsNullOrEmpty(filterExamCode))
                {
                    query = query.Where(t => t.EMS_SUBKEY.StartsWith(subKey) && t.EMS_VENUE == venue);
                    register = register.Where(t => t.EMS_SUBKEY.StartsWith(subKey));

                    foreach (var item in register)
                    {
                        //var ex = await _examinerRepository.GetExaminer(item.IDNumber, item.EMS_SUBKEY, item.ExaminerCode);
                        //var venueregion = await _venueRepository.GetVenueByNameID(venue);
                        //if (ex != null && venueregion != null)
                        //{
                        //    //if(ex.EMS_MARKING_REG_CODE == venueregion.Region)
                        //    //{
                        //    finalRegisterList.Add(item);
                        //    //}
                        //}
                    }
                }

                var appcount = 0;
                var pencount = 0;
                var subjman = 0;
                var censup = 0;
                var peer = 0;
                var acc = 0;
                var total = query.Count();
                var totalpresent = finalRegisterList.Count(t => t.Status == "Present");
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);


                // User is authenticated, you can access properties of the current user
                string username = currentUser.UserName;
                string email = currentUser.Email;
                string idnumber = currentUser.IDNumber;

                var userRoles = await _userManager.GetRolesAsync(currentUser);


                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    appcount = query.Count(t => t.SUBJECT_MANAGER_STATUS == "Recommended");
                    pencount = query.Count(t => t.SUBJECT_MANAGER_STATUS == "Pending");


                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {
                    appcount = query.Count(t => t.CENTRE_SUPERVISOR_STATUS == "Approved");
                    pencount = query.Count(t => t.CENTRE_SUPERVISOR_STATUS == "Pending");
                    subjman = query.Count(t => t.SUBJECT_MANAGER_STATUS == "Pending");

                }
                else if (userRoles != null && userRoles.Contains("Accounts"))
                {
                    appcount = query.Count(t => t.ACCOUNTS_STATUS == "Approved");
                    pencount = query.Count(t => t.ACCOUNTS_STATUS == "Pending");
                    subjman = query.Count(t => t.SUBJECT_MANAGER_STATUS == "Pending");
                    censup = query.Count(t => t.CENTRE_SUPERVISOR_STATUS == "Pending");
                    peer = query.Count(t => t.ACCOUNTS_REVIEW == "Pending");

                }
                else if (userRoles != null && userRoles.Contains("PeerReviewer"))
                {
                    appcount = query.Count(t => t.ACCOUNTS_REVIEW == "Approved");
                    pencount = query.Count(t => t.ACCOUNTS_REVIEW == "Pending");
                    subjman = query.Count(t => t.SUBJECT_MANAGER_STATUS == "Pending");
                    censup = query.Count(t => t.CENTRE_SUPERVISOR_STATUS == "Pending");
                    acc = query.Count(t => t.ACCOUNTS_STATUS == "Approved");
                }
                else if (userRoles != null && userRoles.Contains("Admin"))
                {
                    appcount = query.Count(t => t.STATUS == "Approved");
                    pencount = query.Count(t => t.STATUS == "Pending");
                    subjman = query.Count(t => t.SUBJECT_MANAGER_STATUS == "Pending");
                    censup = query.Count(t => t.CENTRE_SUPERVISOR_STATUS == "Pending");
                    peer = query.Count(t => t.ACCOUNTS_REVIEW == "Pending");
                    acc = query.Count(t => t.ACCOUNTS_STATUS == "Approved");

                }
                var reportData = new
                {
                    SubjectCode = filterSubjectCode.Substring(3) + "/" + filterPaperCode,
                    Venue = venue,
                    ApprovedCount = appcount,
                    PendingCount = pencount,
                    TotalCount = total,
                    PaperCode = filterPaperCode,
                    PresentExaminers = totalpresent,
                    SubjectManagerStats = subjman,
                    CentreSupervisorStats = censup,
                    AccountsStats = acc,
                    PeerReviewerStats = peer,
                };

                return Json(reportData);
            }

            [Authorize]
            public async Task<IActionResult> MissingPeopleReport(string filterExamCode, string filterSubjectCode, string filterPaperCode, string venue, string filterStatus, int page = 1, int pageSize = 10000)
            {
                var query = await _tandSRepository.GetAllTandS();
                var register = await _registerRepository.GetAllRegister();
                var subKey = filterSubjectCode + filterPaperCode;

                if (!string.IsNullOrEmpty(subKey))
                {
                    query = query.Where(t => t.EMS_SUBKEY.StartsWith(subKey) && t.EMS_VENUE == venue);
                }

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);


                // User is authenticated, you can access properties of the current user
                string username = currentUser.UserName;
                string email = currentUser.Email;
                string idnumber = currentUser.IDNumber;

                var userRoles = await _userManager.GetRolesAsync(currentUser);


                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.STATUS = item.SUBJECT_MANAGER_STATUS;
                        }


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Absent")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from the register that are marked as Absent
                        var absentInRegister = register
                            .Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.RegisterStatus == "Absent")
                            .ToList();

                        // Find Absent entries that are also present in the filtered T&S data
                        var absentWithTandS = absentInRegister
                            .Where(r => tandSIds.Contains(r.EMS_EXAMINER_CODE))
                            .ToList();


                        List<TandS> missing = new List<TandS>();
                        foreach (var item in absentWithTandS)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "T and S Found",


                                    };
                                    missing.Add(newtands);
                                }

                                //}

                            }


                        }

                        query = missing;


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.RegisterStatus == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.EMS_EXAMINER_CODE)).ToList();

                        List<TandS> missing = new List<TandS>();
                        foreach (var item in missingEntries)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "Missing",


                                    };
                                    missing.Add(newtands);
                                }

                                //}

                            }


                        }

                        query = missing;


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.SUBJECT_MANAGER_STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.SUBJECT_MANAGER_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Recommended")
                    {
                        query = query.Where(t => t.SUBJECT_MANAGER_STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.SUBJECT_MANAGER_STATUS;
                        }
                    }
                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.STATUS = item.CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Absent")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from the register that are marked as Absent
                        var absentInRegister = register
                            .Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.RegisterStatus == "Absent")
                            .ToList();

                        // Find Absent entries that are also present in the filtered T&S data
                        var absentWithTandS = absentInRegister
                            .Where(r => tandSIds.Contains(r.EMS_EXAMINER_CODE))
                            .ToList();


                        List<TandS> missing = new List<TandS>();
                        foreach (var item in absentWithTandS)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "T and S Found",


                                    };
                                    missing.Add(newtands);
                                }

                                //}

                            }


                        }

                        query = missing;


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {

                        var filteredTandSData = query.ToList();
                        filteredTandSData = filteredTandSData.Where(t => t.EMS_VENUE == venue).ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.RegisterStatus == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.EMS_EXAMINER_CODE)).ToList();

                        List<TandS> missing = new List<TandS>();
                        foreach (var item in missingEntries)
                        {

                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "Missing",


                                    };
                                    missing.Add(newtands);
                                    //}

                                }

                            }
                        }

                        query = missing;


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.CENTRE_SUPERVISOR_STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.CENTRE_SUPERVISOR_STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.CENTRE_SUPERVISOR_STATUS;
                        }
                    }

                }
                else if (userRoles != null && userRoles.Contains("Accounts"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.STATUS = item.ACCOUNTS_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Absent")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from the register that are marked as Absent
                        var absentInRegister = register
                            .Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.RegisterStatus == "Absent")
                            .ToList();

                        // Find Absent entries that are also present in the filtered T&S data
                        var absentWithTandS = absentInRegister
                            .Where(r => tandSIds.Contains(r.EMS_EXAMINER_CODE))
                            .ToList();


                        List<TandS> missing = new List<TandS>();
                        foreach (var item in absentWithTandS)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "T and S Found",


                                    };
                                    missing.Add(newtands);
                                }

                                //}

                            }


                        }

                        query = missing;


                    }

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.RegisterStatus == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.EMS_EXAMINER_CODE)).ToList();

                        List<TandS> missing = new List<TandS>();
                        foreach (var item in missingEntries)
                        {

                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "Missing",


                                    };
                                    missing.Add(newtands);
                                    //}

                                }

                            }
                        }

                        query = missing;
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.ACCOUNTS_STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.ACCOUNTS_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.ACCOUNTS_STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.ACCOUNTS_STATUS;
                        }
                    }
                }
                else if (userRoles != null && userRoles.Contains("PeerReviewer"))
                {

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.STATUS = item.ACCOUNTS_REVIEW;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Absent")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from the register that are marked as Absent
                        var absentInRegister = register
                            .Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.RegisterStatus == "Absent")
                            .ToList();

                        // Find Absent entries that are also present in the filtered T&S data
                        var absentWithTandS = absentInRegister
                            .Where(r => tandSIds.Contains(r.EMS_EXAMINER_CODE))
                            .ToList();


                        List<TandS> missing = new List<TandS>();
                        foreach (var item in absentWithTandS)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "T and S Found",


                                    };
                                    missing.Add(newtands);
                                }

                                //}

                            }


                        }

                        query = missing;


                    }

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.RegisterStatus == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.EMS_EXAMINER_CODE)).ToList();

                        List<TandS> missing = new List<TandS>();
                        foreach (var item in missingEntries)
                        {

                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "Missing",


                                    };
                                    missing.Add(newtands);
                                    //}

                                }

                            }
                        }

                        query = missing;
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.ACCOUNTS_REVIEW;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.ACCOUNTS_REVIEW == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.ACCOUNTS_REVIEW;
                        }
                    }
                }
                else if (userRoles != null && userRoles.Contains("Admin"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.STATUS = item.STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Absent")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from the register that are marked as Absent
                        var absentInRegister = register
                            .Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.RegisterStatus == "Absent")
                            .ToList();

                        // Find Absent entries that are also present in the filtered T&S data
                        var absentWithTandS = absentInRegister
                            .Where(r => tandSIds.Contains(r.EMS_EXAMINER_CODE))
                            .ToList();


                        List<TandS> missing = new List<TandS>();
                        foreach (var item in absentWithTandS)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "T and S Found",


                                    };
                                    missing.Add(newtands);
                                }

                                //}

                            }


                        }

                        query = missing;


                    }

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.RegisterStatus == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.EMS_EXAMINER_CODE)).ToList();

                        List<TandS> missing = new List<TandS>();
                        foreach (var item in missingEntries)
                        {

                            var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                            var currentVanue = await _venueRepository.GetVenueByNameID(venue);

                            if (examiner != null)
                            {
                                if (currentVanue != null)
                                {

                                    //if (examiner.EMS_MARKING_REG_CODE == currentVanue.Region)
                                    //{
                                    var newtands = new TandS()
                                    {

                                        //EMS_EXAMINER_CODE = item.ExaminerCode,

                                        //EMS_NATIONAL_ID = item.IDNumber,

                                        //STATUS = "Missing",


                                    };
                                    missing.Add(newtands);
                                    //}

                                }

                            }
                        }

                        query = missing;
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.STATUS == filterStatus);
                        foreach (var item in query)
                        {
                            item.STATUS = item.STATUS;
                        }
                    }
                }


                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var reportData = new List<MissingPeopleReportViewModel>();
                var reportData1 = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MissingPeopleReportViewModel
                    {

                        EMS_EXAMINER_CODE = item.EMS_EXAMINER_CODE,

                        EMS_NATIONAL_ID = item.EMS_NATIONAL_ID,

                        STATUS = item.STATUS
                    })
                    .ToList();

                foreach (var item in reportData1)
                {
                    var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                    if (examiner != null)
                    {
                        item.EMS_FullName = examiner.EMS_EXAMINER_NAME + " " + examiner.EMS_LAST_NAME;
                        item.Phone = examiner.EMS_PHONE_HOME;
                    }
                    reportData.Add(item);
                }



                var result = new
                {
                    Data = reportData,
                    TotalPages = totalPages,
                    CurrentPage = page
                };

                return Json(result);
            }

            [Authorize]
            public async Task<IActionResult> RegisterReport(string filterExamCode, string filterSubjectCode, string filterPaperCode, string regionCode, string filterStatus, int page = 1, int pageSize = 10000)
            {
                var query = await _registerRepository.GetAllRegister();
                var subKey = filterSubjectCode + filterPaperCode;

                if (!string.IsNullOrEmpty(subKey))
                {
                    query = query.Where(t => t.EMS_SUBKEY.StartsWith(subKey));
                }



                if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                {
                    query = query.ToList();
                }

                else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Absent")
                {
                    query = query.Where(t => t.RegisterStatus == filterStatus);
                }
                else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Present")
                {
                    query = query.Where(t => t.RegisterStatus == filterStatus);
                }




                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var reportData = new List<RegisterReportViewModel>();
                var reportData1 = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new RegisterReportViewModel
                    {

                        //ExaminerCode = item.ExaminerCode,
                        //SubKey = item.EMS_SUBKEY,
                        //IDNumber = item.IDNumber,
                        //AttendanceStatus = item.AttendanceStatus,
                        //RecommendedStatus = item.RecommendedStatus,

                        //Status = item.Status
                    })
                    .ToList();
                foreach (var item in reportData1)
                {
                    var examiner = await _examinerRepository.GetExaminer(item.IDNumber, item.SubKey, item.ExaminerCode);
                    if (examiner != null)
                    {
                        item.FirstName = examiner.EMS_EXAMINER_NAME + " " + examiner.EMS_LAST_NAME;
                    }
                    reportData.Add(item);
                }
                var result = new
                {
                    Data = reportData,
                    TotalPages = totalPages,
                    CurrentPage = page
                };

                return Json(result);
            }




            [Authorize]
            public async Task<IActionResult> ScriptReport(string filterExamCode, string filterSubjectCode, string filterPaperCode, string regionCode)
            {
                var query = new List<ExaminerScriptsMarked>();
                var querydata = await _transactionRepository.GetAllTransactionExaminer();
                var register = await _registerRepository.GetAllRegister();
                var subKey = filterSubjectCode + filterPaperCode;
                var finalRegisterList = new List<RegisterDto>();

                if (!string.IsNullOrEmpty(filterExamCode))
                {
                    query = query.Where(t => t.EMS_SUBKEY.StartsWith(subKey)).ToList();
                    register = register.Where(t => t.EMS_SUBKEY.StartsWith(subKey));

                    foreach (var item in register)
                    {
                        //var ex = await _examinerRepository.GetExaminer(item.IDNumber, item.EMS_SUBKEY, item.ExaminerCode);

                        ////if (ex != null && regionCode != null)
                        ////{
                        ////if (ex.EMS_MARKING_REG_CODE == regionCode)
                        ////{
                        //finalRegisterList.Add(item);
                        //}
                        //}
                    }

                    foreach (var item in querydata)
                    {
                        var ex = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);

                        //if (ex != null && regionCode != null)
                        //{
                        //    if (ex.EMS_MARKING_REG_CODE == regionCode)
                        //    {
                        query.Add(item);
                        //    }
                        //}
                    }
                }

                var appcount = 0;
                var pencount = 0;
                var missing = 0;
                var available = 0;
                var subjman = 0;
                var censup = 0;
                var pmspending = 0;
                var total = query.Count();
                var totalpresent = finalRegisterList.Count(t => t.Status == "Present");
                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);


                // User is authenticated, you can access properties of the current user
                string username = currentUser.UserName;
                string email = currentUser.Email;
                string idnumber = currentUser.IDNumber;

                var userRoles = await _userManager.GetRolesAsync(currentUser);


                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    appcount = query.Count(t => t.EMS_CERTIFIED_STATUS == "Certified");
                    pencount = query.Count(t => t.EMS_CERTIFIED_STATUS == "Pending");
                    pmspending = query.Count(t => t.EMS_APPROVED_STATUS == "Pending");

                    available = query.Count(t => t.SCRIPTS_MARKED != null);

                    var filteredTransData = query.ToList();
                    var transIds = filteredTransData.Select(t => t.EMS_EXAMINER_CODE).ToList();
                    var presentInRegister = finalRegisterList.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.Status == "Present").ToList();
                    var missingEntries = presentInRegister.Where(r => !transIds.Contains(r.ExaminerCode)).ToList();
                    missing = missingEntries.Count();

                }
                else if (userRoles != null && userRoles.Contains("DPMS") || userRoles.Contains("RPMS") || userRoles.Contains("PMS"))
                {
                    appcount = query.Count(t => t.EMS_APPROVED_STATUS == "Approved");
                    pencount = query.Count(t => t.EMS_APPROVED_STATUS == "Pending");
                    missing = query.Count(t => t.SCRIPTS_MARKED == null);
                    available = query.Count(t => t.SCRIPTS_MARKED != null);
                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {
                    appcount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved");
                    pencount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending");

                    available = query.Count(t => t.SCRIPTS_MARKED != null);

                    var filteredTransData = query.ToList();
                    var transIds = filteredTransData.Select(t => t.EMS_EXAMINER_CODE).ToList();
                    var presentInRegister = finalRegisterList.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.Status == "Present").ToList();
                    var missingEntries = presentInRegister.Where(r => !transIds.Contains(r.ExaminerCode)).ToList();
                    missing = missingEntries.Count();

                }
                else if (userRoles != null && userRoles.Contains("Accounts"))
                {
                    appcount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved");
                    pencount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending");

                    available = query.Count(t => t.SCRIPTS_MARKED != null);

                    var filteredTransData = query.ToList();
                    var transIds = filteredTransData.Select(t => t.EMS_EXAMINER_CODE).ToList();
                    var presentInRegister = finalRegisterList.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.Status == "Present").ToList();
                    var missingEntries = presentInRegister.Where(r => !transIds.Contains(r.ExaminerCode)).ToList();
                    missing = missingEntries.Count();

                }
                else if (userRoles != null && userRoles.Contains("PeerReviewer"))
                {
                    appcount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved");
                    pencount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending");

                    available = query.Count(t => t.SCRIPTS_MARKED != null);

                    var filteredTransData = query.ToList();
                    var transIds = filteredTransData.Select(t => t.EMS_EXAMINER_CODE).ToList();
                    var presentInRegister = finalRegisterList.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.Status == "Present").ToList();
                    var missingEntries = presentInRegister.Where(r => !transIds.Contains(r.ExaminerCode)).ToList();
                    missing = missingEntries.Count();
                }
                else if (userRoles != null && userRoles.Contains("Admin"))
                {
                    appcount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved");
                    pencount = query.Count(t => t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending");

                    available = query.Count(t => t.SCRIPTS_MARKED != null);
                    var filteredTransData = query.ToList();
                    var transIds = filteredTransData.Select(t => t.EMS_EXAMINER_CODE).ToList();
                    var presentInRegister = finalRegisterList.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.Status == "Present").ToList();
                    var missingEntries = presentInRegister.Where(r => !transIds.Contains(r.ExaminerCode)).ToList();
                    missing = missingEntries.Count();

                }
                var reportData = new
                {
                    SubjectCode = filterSubjectCode.Substring(3) + "/" + filterPaperCode,
                    RegionCode = regionCode,
                    ApprovedCount = appcount,
                    PendingCount = pencount,
                    TotalCount = total,
                    PaperCode = filterPaperCode,
                    PresentExaminers = totalpresent,
                    Missing = missing,
                    Available = available,
                    Pmspending = pmspending,
                    SubjectManagerStats = subjman,

                };

                return Json(reportData);
            }

            [Authorize]
            public async Task<IActionResult> MissingScriptReport(string filterExamCode, string filterSubjectCode, string filterPaperCode, string filterStatus, string regionCode, int page = 1, int pageSize = 10000)
            {
                var querydata = await _transactionRepository.GetAllTransactionExaminer();
                var registerdata = await _registerRepository.GetAllRegister();
                List<ExaminerScriptsMarked> query = new List<ExaminerScriptsMarked>();
                List<RegisterDto> register = new List<RegisterDto>();
                foreach (var item in querydata)
                {
                    var eximiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                    if (eximiner != null)
                    {
                        //if(eximiner.EMS_MARKING_REG_CODE == regionCode)
                        //{
                        query.Add(item);
                        //}
                    }
                }


                foreach (var item in registerdata)
                {
                    //var eximiner = await _examinerRepository.GetExaminer(item.IDNumber, item.EMS_SUBKEY, item.ExaminerCode);
                    //if (eximiner != null)
                    //{
                    //    //if (eximiner.EMS_MARKING_REG_CODE == regionCode)
                    //    //{
                    //    register.Add(item);
                    //    //}
                    //}
                }


                var subKey = filterSubjectCode + filterPaperCode;

                if (!string.IsNullOrEmpty(subKey))
                {
                    query = query.Where(t => t.EMS_SUBKEY.StartsWith(subKey)).ToList();
                }

                ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);


                // User is authenticated, you can access properties of the current user
                string username = currentUser.UserName;
                string email = currentUser.Email;
                string idnumber = currentUser.IDNumber;

                var userRoles = await _userManager.GetRolesAsync(currentUser);


                if (userRoles != null && userRoles.Contains("SubjectManager"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS;
                        }


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTransData = query.ToList();
                        var tranSIds = filteredTransData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey) && r.Status == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tranSIds.Contains(r.ExaminerCode)).ToList();

                        List<ExaminerScriptsMarked> missing = new List<ExaminerScriptsMarked>();
                        foreach (var item in missingEntries)
                        {
                            var examiner = await _examinerRepository.GetExaminer(item.IDNumber, item.EMS_SUBKEY, item.ExaminerCode);
                            var newtands = new ExaminerScriptsMarked()
                            {

                                EMS_EXAMINER_CODE = item.ExaminerCode,
                                EMS_SUBKEY = item.EMS_SUBKEY,


                                EMS_NATIONAL_ID = item.IDNumber,




                            };
                            missing.Add(newtands);
                        }

                        query = missing;


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.EMS_CERTIFIED_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Certified")
                    {
                        query = query.Where(t => t.EMS_CERTIFIED_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS;
                        }
                    }
                }
                else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {

                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.Status == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.ExaminerCode)).ToList();

                        List<ExaminerScriptsMarked> missing = new List<ExaminerScriptsMarked>();
                        foreach (var item in missingEntries)
                        {

                            var newtands = new ExaminerScriptsMarked()
                            {

                                EMS_EXAMINER_CODE = item.ExaminerCode,

                                EMS_NATIONAL_ID = item.IDNumber,

                                EMS_SUBKEY = item.EMS_SUBKEY


                            };
                            missing.Add(newtands);
                        }

                        query = missing;


                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }

                }
                else if (userRoles != null && userRoles.Contains("Accounts"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.Status == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.ExaminerCode)).ToList();

                        List<ExaminerScriptsMarked> missing = new List<ExaminerScriptsMarked>();
                        foreach (var item in missingEntries)
                        {

                            var newtands = new ExaminerScriptsMarked()
                            {

                                EMS_EXAMINER_CODE = item.ExaminerCode,


                                EMS_NATIONAL_ID = item.IDNumber,
                                EMS_SUBKEY = item.EMS_SUBKEY


                            };
                            missing.Add(newtands);
                        }

                        query = missing;
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                }
                else if (userRoles != null && userRoles.Contains("PeerReviewer"))
                {

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.Status == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.ExaminerCode)).ToList();

                        List<ExaminerScriptsMarked> missing = new List<ExaminerScriptsMarked>();
                        foreach (var item in missingEntries)
                        {

                            var newtands = new ExaminerScriptsMarked()
                            {

                                EMS_EXAMINER_CODE = item.ExaminerCode,


                                EMS_NATIONAL_ID = item.IDNumber,
                                EMS_SUBKEY = item.EMS_SUBKEY


                            };
                            missing.Add(newtands);
                        }

                        query = missing;
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                }
                else if (userRoles != null && userRoles.Contains("Admin"))
                {
                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "All")
                    {
                        query = query.ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }

                    if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Missing")
                    {
                        var filteredTandSData = query.ToList();
                        var tandSIds = filteredTandSData.Select(t => t.EMS_EXAMINER_CODE).ToList();

                        // Get entries from register that are marked as Present
                        var presentInRegister = register.Where(r => r.EMS_SUBKEY.StartsWith(subKey)
                                                                   && r.Status == "Present").ToList();

                        // Find entries that are in the register but not in the filtered T&S data
                        var missingEntries = presentInRegister.Where(r => !tandSIds.Contains(r.ExaminerCode)).ToList();

                        List<ExaminerScriptsMarked> missing = new List<ExaminerScriptsMarked>();
                        foreach (var item in missingEntries)
                        {

                            var newtands = new ExaminerScriptsMarked()
                            {

                                EMS_EXAMINER_CODE = item.ExaminerCode,


                                EMS_NATIONAL_ID = item.IDNumber,
                                EMS_SUBKEY = item.EMS_SUBKEY


                            };
                            missing.Add(newtands);
                        }

                        query = missing;
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Pending")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                    else if (!string.IsNullOrEmpty(filterStatus) && filterStatus == "Approved")
                    {
                        query = query.Where(t => t.EMS_CENTRE_SUPERVISOR_STATUS == filterStatus).ToList();
                        foreach (var item in query)
                        {
                            item.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        }
                    }
                }


                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var reportData = new List<MissingScriptReportViewModel>();
                var reportData1 = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MissingScriptReportViewModel
                    {

                        EMS_EXAMINER_CODE = item.EMS_EXAMINER_CODE,
                        EMS_NATIONAL_ID = item.EMS_NATIONAL_ID,

                        EMS_SUBKEY = item.EMS_SUBKEY,
                        SCRIPTMARKED = item.SCRIPTS_MARKED,
                        SUBSTATUS = item.EMS_CERTIFIED_STATUS ?? string.Empty,
                        PMSSTATUS = item.EMS_APPROVED_STATUS ?? string.Empty,
                        CENTSTATUS = item.EMS_CENTRE_SUPERVISOR_STATUS ?? string.Empty



                    })
                    .ToList();

                foreach (var item in reportData1)
                {
                    var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_EXAMINER_CODE);
                    if (examiner != null)
                    {
                        item.EMS_FullName = examiner.EMS_EXAMINER_NAME + " " + examiner.EMS_LAST_NAME;
                        reportData.Add(item);

                        if (userRoles != null && userRoles.Contains("Admin") || userRoles.Contains("Accounts") || userRoles.Contains("PeerReviewer"))
                        {
                            item.STATUS = item.CENTSTATUS;
                            if (filterStatus == "Missing")
                            {
                                item.STATUS = "Missing";
                                item.SCRIPTMARKED = 0;
                            }
                        }
                        else if (userRoles != null && userRoles.Contains("SubjectManager"))
                        {
                            item.STATUS = item.SUBSTATUS;
                            if (filterStatus == "Missing")
                            {
                                item.STATUS = "Missing";
                                item.SCRIPTMARKED = 0;
                            }
                        }
                        else if (userRoles != null && userRoles.Contains("CentreSupervisor"))
                        {
                            item.STATUS = item.CENTSTATUS;
                            if (filterStatus == "Missing")
                            {
                                item.STATUS = "Missing";
                                item.SCRIPTMARKED = 0;
                            }
                        }
                        else if (userRoles != null && userRoles.Contains("PMS") || userRoles.Contains("RPMS") || userRoles.Contains("DPMS"))
                        {
                            item.STATUS = item.PMSSTATUS;
                            if (filterStatus == "Missing")
                            {
                                item.STATUS = "Missing";
                                item.SCRIPTMARKED = 0;
                            }
                        }
                    }



                }



                var result = new
                {
                    Data = reportData,
                    TotalPages = totalPages,
                    CurrentPage = page
                };

                return Json(result);
            }
        
        [Authorize]
        public IActionResult TravellingAndSubstanceTranscribers()
        {

            //var userSession = new SessionModel();
            //if (!String.IsNullOrEmpty(venue))
            //{
            //    userSession = new SessionModel()
            //    {

            //        Venue = venue,
            //        ExamCode = examCode,
            //        Activity = activity,
                    

            //    };
            //    HttpContext.Session.SetObjectAsJson("Session", userSession);
            //}
            //else
            //{
            //    userSession = HttpContext.Session.GetObjectFromJson<SessionModel>("Session");
            //}

            //ViewBag.Venue = userSession.Venue;
            //ViewBag.Activity = userSession.Activity;
            //ViewBag.ExamCode = userSession.ExamCode;
            return View();
        }


      

     

        [Authorize]
        public IActionResult MasterReport()
        {


            return View();
        }

     

        [Authorize]
        public async Task<IActionResult> MasterReportExaminerPayment()
        {


            List<TravelExaminerMarkingReport> markingReports = new List<TravelExaminerMarkingReport>();
            var fees = await _advanceFeesRepository.GetFirstAdvanceFee();
            string excelFilePath = "data.xlsx";

            var examinersfromtransction = await _transactionRepository.GetAllTransactionExaminer();

            var rateandtax = await _taxRepository.GetFirstTaxAndRate();



            foreach (var item in examinersfromtransction)
            {
                if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Approved" && item.EMS_CERTIFIED_STATUS == "Certified" && item.EMS_CENTRE_SUPERVISOR_STATUS == "Approved" && item.SCRIPTS_MARKED != null)
                {

                    var examiner = await _examinerRepository.GetExaminer(item.EMS_NATIONAL_ID, item.EMS_SUBKEY, item.EMS_SUBKEY);
                    if (examiner != null)
                    {

                        decimal totalAfterCupturing = 0;
                        decimal rate = 0;
                        decimal tax = 0;
                        decimal zig_amount = 0;
                        decimal amt_payable = 0;
                        decimal adjustedTax = 0;
                        decimal totalAfterScript = 0;

                        if (rateandtax != null)
                        {
                            rate = rateandtax.CurrentRate;
                            tax = rateandtax.WHT / 100;
                        }

                        totalAfterScript = item.SCRIPTS_MARKED.GetValueOrDefault() * item.SCRIPT_RATE.GetValueOrDefault();
                        totalAfterCupturing = item.GRAND_TOTAL.GetValueOrDefault() + item.CAPTURING_FEES.GetValueOrDefault();
                        zig_amount = totalAfterCupturing * rate;
                        if (totalAfterCupturing > 1000)
                        {
                            adjustedTax = zig_amount * tax;
                        }


                        var subjectCode1 = examiner.EMS_SUB_SUB_ID.Substring(3);
                        var paperCode1 = examiner.EMS_PAPER_CODE;
                        var subjectName = $"{subjectCode1}/{paperCode1}";

                        amt_payable = zig_amount - adjustedTax;


                        DateTime currentDate = DateTime.Now;


                        string currentDateAsString = currentDate.ToString("yyyy-MM-dd");

                        var report = new TravelExaminerMarkingReport()
                        {
                            Date = currentDateAsString,
                            BankName = examiner.EMS_BANK_NAME_ZWL,
                            BankAccount = examiner.EMS_ACCOUNT_NO_ZWL,
                            ShortCode = examiner.EMS_BRANCH_CODE_ZWL,
                            Fullname = examiner.EMS_EXAMINER_NAME + " " + examiner.EMS_LAST_NAME,
                            Status = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                            Subject = subjectName,
                            ScriptMarked = item.SCRIPTS_MARKED.ToString(),
                            ScriptRate = item.SCRIPT_RATE.ToString(),
                            TotalAfterScriptRate = totalAfterScript.ToString(),
                            Resp = item.RESPONSIBILITY_FEES.ToString(),
                            Coord = item.COORDINATION_FEES.ToString(),
                            GrandTotal = item.GRAND_TOTAL.ToString(),
                            Capturing = item.CAPTURING_FEES.ToString(),
                            Total = totalAfterCupturing.ToString(),
                            Rate = rate.ToString(),
                            ZIGAmount = zig_amount.ToString(),
                            WHT = adjustedTax.ToString(),
                            AmountPayable = amt_payable.ToString(),

                        };

                        markingReports.Add(report);

                    }

                }
            }

            CsvService csvService = new CsvService();
            await csvService.ExportToExcelExaminer(markingReports, excelFilePath);


            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(excelFilePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var fileName = $"master_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"; // Generates a filename like "data_20240908_123456.xlsx"
            return File(fileBytes, contentType, fileName);
        }


        [Authorize]
        public IActionResult TandSCheckReport()
        {
            return View();
        }

        [Authorize]
        public IActionResult TandSAccountsReport()
        {
            return View();
        }


        [Authorize]
        public IActionResult EPSAccountsReport()
        {
            return View();
        }

        [Authorize]
        public IActionResult ReportRegister()
        {
            return View();
        }




        [Authorize]
        public IActionResult ScriptsMarkedCheckReport()
        {
            return View();
        }

        [Authorize]
        public IActionResult CheckList()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTablesRequest request)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
            var venue = request.Venue;
            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Get search value
            var searchValue = request.Search?.Value;

            // Get sorting information
            var sortColumn = request.Order?[0]?.Column; // Column to sort
            var sortDirection = request.Order?[0]?.Dir; // Sort direction (asc/desc)
            var sortColumnName = request.Columns?[sortColumn ?? 0]?.Name; // Column name to sort by

            // Query your data based on the filters, search, and sorting
            var data = await _reportRepository.GetFilteredData(
                filterStatus,
                venue,
                activity,
                examCode,
                subject,
                paperCode,
                regionCode,
                currentUser);

            // Apply client-side search if search value exists
            var filteredData = data.Results.AsQueryable();

            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchTerm = request.Search.Value.ToLower(); // Renamed variable
                filteredData = filteredData.Where(x =>
                    (x.Subject != null && x.Subject.ToLower().Contains(searchTerm)) ||
                    (x.FirstName != null && x.FirstName.ToLower().Contains(searchTerm)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(searchTerm))
                );
            }

            // Apply sorting


            // Get counts after filtering
            var recordsFiltered = filteredData.Count();



            // Return the data in the format expected by DataTables
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = data.TotalRecords,
                recordsFiltered = data.FilteredRecords,
                data = data.Results
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetAccountsReportData([FromBody] DataTablesReportRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
            var venue = request.Venue;
            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;


            //var data = await _reportRepository.GetFilteredAccountsData(filterStatus, venue, activity, examCode, subject, paperCode, regionCode, currentUser);

            var query = _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
     .Where(s => _context.EXAMINER_TRANSACTIONS
         .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present"))
     .Include(s => s.Examiner)
     .Include(s => s.Examiner.ExaminerScriptsMarkeds)
     .Include(s => s.TandSDetails)
     .Include(s => s.TandSAdvance)
     .Include(s => s.TandSFiles)
     .AsQueryable();

            // Apply filters (excluding filterStatus for now)
            if (venue != null && venue.Any())
            {
                query = query.Where(t => venue.Contains(t.EMS_VENUE));
            }

            if (activity != null && activity.Any())
            {
                query = query.Where(t => activity.Contains(t.EMS_PURPOSEOFJOURNEY));
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


            if (filterStatus != null && filterStatus.Any() && !filterStatus.Contains("All"))
            {
                query = query.Where(t =>
                    filterStatus.Contains("Pending") && (t.ACCOUNTS_STATUS == "Pending" || t.ACCOUNTS_REVIEW == "Pending") ||
                    filterStatus.Contains("Approved") && t.ACCOUNTS_REVIEW == "Approved" && t.ACCOUNTS_STATUS == "Approved" ||
                    filterStatus.Contains("Paid") && t.PaidStatus == "Paid" ||
                    filterStatus.Contains("NotPaid") && t.PaidStatus == "NotPaid");
                
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
                        SubKey = t.EMS_SUBKEY,
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
                        PaidStatus = t.PaidStatus,
                        Status = t.ACCOUNTS_REVIEW,
                        Venue = t.EMS_VENUE

                    };


                    report.Add(newReport);
                }
            }

           var data = new ReportAccountsDataResult()
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };


            var filteredData = data.Results.AsQueryable();

            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchTerm = request.Search.Value.ToLower(); // Renamed variable
                filteredData = filteredData.Where(x =>
                    (x.Subject != null && x.Subject.ToLower().Contains(searchTerm)) ||
                    (x.FirstName != null && x.FirstName.ToLower().Contains(searchTerm)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(searchTerm))
                );
            }
            // Return the data in the format expected by DataTables
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = data.TotalRecords,
                recordsFiltered = data.FilteredRecords,
                data = data.Results
            });
        }



        [HttpPost]
        public async Task<IActionResult> GetAccountsEPSReportData([FromBody] DataTablesReportRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
            var venue = request.Venue;
            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;


            //// Query your data based on the filters
            //var data = await _reportRepository.GetFilteredAccountsEPSData(filterStatus, venue, examCode, subject, paperCode, regionCode, currentUser);


            var query = _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_ACTIVITY == "BEM" && a.IsPresent && a.RegisterStatus == "Present" && (a.EMS_ECT_EXAMINER_CAT_CODE != "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE != "BT" || a.EMS_ECT_EXAMINER_CAT_CODE != "S" || a.EMS_ECT_EXAMINER_CAT_CODE != "I" || a.EMS_ECT_EXAMINER_CAT_CODE != "A"))
          .Include(a => a.Examiner)
          .AsQueryable();

            // Apply filters (excluding filterStatus for now)
            if (venue != null && venue.Any())
            {
                query = query.Where(t => venue.Contains(t.EMS_VENUE));
            }

            //if (activity != null && activity.Any())
            //{
            //    query = query.Where(t => activity.Contains(t.EMS_PURPOSEOFJOURNEY));
            //}

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

            if (filterStatus != null && filterStatus.Any() && !filterStatus.Contains("All"))
            {
                query = query.Where(t =>
                    filterStatus.Contains("Pending") && t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending" ||
                    filterStatus.Contains("Approved") && t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved" ||
                    filterStatus.Contains("Paid") && t.PaidStatus == "Paid" ||
                    filterStatus.Contains("NotPaid") && t.PaidStatus == "NotPaid"
                );
            }

            // Get the total number of records (before pagination)
            var totalRecords = query.Count();


            // Map the results to ReportData
            var report = new List<ReportAccountsEPSData>();
            foreach (var t in query)
            {

                var newReport = new ReportAccountsEPSData()
                {
                    SubKey =  t.EMS_SUBKEY,
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
            var data = new ReportAccountsEPSDataResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = report.Count,
                Results = report
            };

            var filteredData = data.Results.AsQueryable();

            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchTerm = request.Search.Value.ToLower(); // Renamed variable
                filteredData = filteredData.Where(x =>
                    (x.Subject != null && x.Subject.ToLower().Contains(searchTerm)) ||
                    (x.FirstName != null && x.FirstName.ToLower().Contains(searchTerm)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(searchTerm))
                );
            }

            // Return the data in the format expected by DataTables
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = data.TotalRecords,
                recordsFiltered = data.FilteredRecords,
                data = data.Results
            });
        }



        [HttpPost]
        public async Task<IActionResult> GetScriptsData([FromBody] DataTablesRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
        
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Query your data based on the filters
            var data = await _reportRepository.GetScriptsFilteredData(filterStatus,examCode, subject, paperCode, regionCode,currentUser);

            var filteredData = data.Results.AsQueryable();

            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchTerm = request.Search.Value.ToLower(); // Renamed variable
                filteredData = filteredData.Where(x =>
                    (x.Subject != null && x.Subject.ToLower().Contains(searchTerm)) ||
                    (x.FirstName != null && x.FirstName.ToLower().Contains(searchTerm)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(searchTerm))
                );
            }

            // Return the data in the format expected by DataTables
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = data.TotalRecords,
                recordsFiltered = data.FilteredRecords,
                data = data.Results
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetRegisterData([FromBody] DataTablesRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Query your data based on the filters
            var data = await _reportRepository.GetRegisterFilteredData(filterStatus,activity ,examCode, subject, paperCode, regionCode, currentUser);

            var filteredData = data.Results.AsQueryable();

            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchTerm = request.Search.Value.ToLower(); // Renamed variable
                filteredData = filteredData.Where(x =>
                    (x.Subject != null && x.Subject.ToLower().Contains(searchTerm)) ||
                    (x.FirstName != null && x.FirstName.ToLower().Contains(searchTerm)) ||
                    (x.LastName != null && x.LastName.ToLower().Contains(searchTerm))
                );
            }

            // Return the data in the format expected by DataTables
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = data.TotalRecords,
                recordsFiltered = data.FilteredRecords,
                data = data.Results
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetCheckListStatusReport([FromBody] DataTablesRequest request)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);

            // Access filter values

            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;
            var venue = request.Venue;

            var subkey = request.Subject + request.PaperCode;
            var checkTandSlistForPresentExaminers = new List<TandS>();


            var checkScriptslistForPresentExaminers = new List<ExaminerScriptsMarked>();


            var presentExaminers = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == request.Subject && a.EMS_PAPER_CODE == request.PaperCode && a.RegisterStatus == "Present" && a.EMS_ACTIVITY == request.Activity).
                       ToListAsync();

            var presentScripts = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == request.Subject && a.EMS_PAPER_CODE == request.PaperCode && a.RegisterStatus == "Present" && a.EMS_ACTIVITY == request.Activity && a.IsPresent && a.SCRIPTS_MARKED > 0).
                       ToListAsync();

            var presentTandS = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                  .Where(s => s.EMS_VENUE == venue && s.EMS_SUBKEY.StartsWith(subkey) &&
                              _context.EXAMINER_TRANSACTIONS
                                  .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present"))
                  .Include(a => a.Examiner)
                  .ToListAsync();



            foreach (var item in presentExaminers)
            {
                var tands = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(a => a.EMS_SUBKEY == item.EMS_SUBKEY);

                if (tands != null)
                {
                    checkTandSlistForPresentExaminers.Add(tands);
                }


                if (item.SCRIPTS_MARKED != 0 && item.IsPresent)
                {
                    checkScriptslistForPresentExaminers.Add(item);
                }


            }

            // ✅ Calculate percentages
            double totalPresent = presentExaminers.Count;

            double percentagetands = totalPresent > 0
                ? checkTandSlistForPresentExaminers.Count / totalPresent * 100
                : 0;

            double percentageScripts = totalPresent > 0
                ? checkScriptslistForPresentExaminers.Count / totalPresent * 100
                : 0;

            // Optional: Round if needed
            percentagetands = Math.Round(percentagetands, 2);
            percentageScripts = Math.Round(percentageScripts, 2);
            var overalPercentage = "Pending";
            var tandSApprovalStatus = "Pending";
            var scriptsApprovalStatus = "Pending";
            if (percentagetands >= 100 && percentageScripts >= 100)
            {
                overalPercentage = "✓";
            }

            if (percentagetands >= 100)
            {
                tandSApprovalStatus = "✓";
            }
            if(percentageScripts >= 100)
            {
                scriptsApprovalStatus = "✓";
            }

            var modelList = new List<CheckListReport>();
            var checkListPercentage = new CheckListReport()
            {
                Subject = request.Subject.Substring(3,4) + "/" + request.PaperCode,
                TandSPercentage = percentagetands.ToString(),
                ScriptsMarkedPercentage = percentageScripts.ToString(),
                overallStatus = overalPercentage,
                ScriptsApprovalStatus = scriptsApprovalStatus,
                TandSApprovalStatus = tandSApprovalStatus
            };
            modelList.Add(checkListPercentage);
            return Json(new
            {
                presentExaminers = presentExaminers.Count,
                presentTandS = presentTandS.Count,
                presentScripts = presentScripts.Count,
                draw = request.Draw,
                recordsTotal = modelList.Count,
                recordsFiltered = modelList.Count,
                data = modelList
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DownloadTandSReportCSV([FromBody] DataTablesReportRequest request)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
            var venue = request.Venue;
            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;

            // Generate the Excel file
            //string filePath = await ClaculateAdvances(examCode, subject, paperCode, venue, activity, regionCode);

            string excelFilePath = "data.xlsx";

            List<TravelAdvanceReport> accountsReports = new List<TravelAdvanceReport>();

            


         var   query = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
              .Where(et =>  _context.EXAMINER_TRANSACTIONS
                                    .Any(a => a.EMS_SUBKEY == et.EMS_SUBKEY && a.RegisterStatus == "Present"))
              .Include(a => a.Examiner)
                .Include(t => t.TandSDetails)
                .Include(t => t.TandSAdvance)
        .ToListAsync();

            // Apply filters (excluding filterStatus for now)
            if (venue != null && venue.Any())
            {
                query = query.Where(t => venue.Contains(t.EMS_VENUE)).ToList();
            }

            if (activity != null && activity.Any())
            {
                query = query.Where(t => activity.Contains(t.EMS_PURPOSEOFJOURNEY)).ToList();
            }

            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode)).ToList();
            }



            if (!string.IsNullOrEmpty(subject))
            {
                var sub = subject.Substring(3);
                query = query.Where(t => t.EMS_SUBKEY.Substring(3, 4) == sub).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.Substring(7, 2) == paperCode).ToList();
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
                query = tandSData.ToList();
            }

            if (filterStatus != null && filterStatus.Any() && !filterStatus.Contains("All"))
            {
                query = query.Where(t =>
                    filterStatus.Contains("Pending") && (t.ACCOUNTS_STATUS == "Pending" || t.ACCOUNTS_REVIEW == "Pending") ||
                    filterStatus.Contains("Approved") && t.ACCOUNTS_REVIEW == "Approved" && t.ACCOUNTS_STATUS == "Approved" ||
                    filterStatus.Contains("Paid") && t.PaidStatus == "Paid" ||
                    filterStatus.Contains("NotPaid") && t.PaidStatus == "NotPaid").ToList();

            }


            //var tandsByComponent = query.Where(x => x.ACCOUNTS_REVIEW == "Approved" && x.ACCOUNTS_STATUS == "Approved").ToList();

            accountsReports = await MakeCulculationForAdvances(query);

            CsvService csvService = new CsvService();
            await csvService.ExportToExcel(accountsReports, excelFilePath);


            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(excelFilePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";



            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"tands_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DownloadEPSReportCSV([FromBody] DataTablesReportRequest request)
        {

            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            // Access filter values from the request object
            var filterStatus = request.FilterStatus;
            var venue = request.Venue;
            var activity = request.Activity;
            var examCode = request.ExamCode;
            var subject = request.Subject;
            var paperCode = request.PaperCode;
            var regionCode = request.RegionCode;
            // Generate the Excel file
            string filePath = await GenerateComponentExaminerAccountsReportExcel(examCode, subject, paperCode, regionCode);

          var  query = await _context.EXAMINER_TRANSACTIONS
                     .Where(a => 
                                 a.EMS_ACTIVITY == "BEM"  &&
                                 a.RegisterStatus == "Present" && a.IsPresent  && (a.EMS_ECT_EXAMINER_CAT_CODE != "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE != "BT" || a.EMS_ECT_EXAMINER_CAT_CODE != "S" || a.EMS_ECT_EXAMINER_CAT_CODE != "I" || a.EMS_ECT_EXAMINER_CAT_CODE != "A"))
                     .Include(a => a.Examiner)
                     .ToListAsync();

            string excelFilePath = "data.xlsx";

            // Apply filters (excluding filterStatus for now)
            if (venue != null && venue.Any())
            {
                query = query.Where(t => venue.Contains(t.EMS_VENUE)).ToList();
            }

            //if (activity != null && activity.Any())
            //{
            //    query = query.Where(t => activity.Contains(t.EMS_PURPOSEOFJOURNEY));
            //}

            if (!string.IsNullOrEmpty(examCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.StartsWith(examCode)).ToList();
            }

            if (!string.IsNullOrEmpty(subject))
            {
                var sub = subject.Substring(3);
                query = query.Where(t => t.EMS_SUBKEY.Substring(3, 4) == sub).ToList();
            }

            if (!string.IsNullOrEmpty(paperCode))
            {
                query = query.Where(t => t.EMS_SUBKEY.Substring(7, 2) == paperCode).ToList();
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                query = query.Where(t => t.EMS_MARKING_REG_CODE == regionCode).ToList();
            }

            if (filterStatus != null && filterStatus.Any() && !filterStatus.Contains("All"))
            {
                query = query.Where(t =>
                    filterStatus.Contains("Pending") && t.EMS_CENTRE_SUPERVISOR_STATUS == "Pending" ||
                    filterStatus.Contains("Approved") && t.EMS_CENTRE_SUPERVISOR_STATUS == "Approved" ||
                    filterStatus.Contains("Paid") && t.PaidStatus == "Paid" ||
                    filterStatus.Contains("NotPaid") && t.PaidStatus == "NotPaid"
                ).ToList();
            }


                //var examinersfromtransction = await _transactionRepository.GetComponentExaminer(examCode, subject, paperCode, regionCode);

                List<TravelExaminerMarkingReport> accountsReports = await CalculateExaminerPayment(query);

            CsvService csvService = new CsvService();
            await csvService.ExportToExcelExaminer(accountsReports, excelFilePath);

    

            // Read the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(excelFilePath);

            // Set the MIME type
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Return the file as a downloadable attachment
            return File(fileBytes, contentType, $"eps_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEPSPayment([FromBody] PaymentUpdateDto dto)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var record = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(x => x.EMS_SUBKEY == dto.SubKey);
            if (record == null)
                return NotFound();

            record.PaidAmount = dto.Amount;
            record.PaidStatus = dto.PaidStatus;
            record.PaidStatusDate = DateTime.Now.ToString();
            record.PaidStatusBy = currentUser.UserName;
            _context.Update(record);
            // Optionally add PaidStatusBy = User.Identity.Name;

            await _context.SaveChangesAsync(currentUser.Id);
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> UpdateTandSPayment([FromBody] PaymentUpdateDto dto)
        {
            ApplicationUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
            var record = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(x => x.EMS_SUBKEY == dto.SubKey);
            if (record == null)
                return NotFound();

            record.PaidAmount = dto.Amount;
            record.PaidStatus = dto.PaidStatus;
            record.PaidStatusDate = DateTime.Now.ToString();
            record.PaidStatusBy = currentUser.UserName;
            // Optionally add PaidStatusBy = User.Identity.Name;
            _context.Update(record);

            await _context.SaveChangesAsync(currentUser.Id);
            return Ok();
        }




    }

    public class PaymentUpdateDto
    {
        public string SubKey { get; set; }
        public decimal Amount { get; set; }
        public string PaidStatus { get; set; }
    }
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search Search { get; set; }
        public List<Column> Columns { get; set; }
        public List<Order> Order { get; set; }
        public string FilterStatus { get; set; }
        public string Venue { get; set; }
        public string Activity { get; set; }
        public string ExamCode { get; set; }
        public string Subject { get; set; }
        public string PaperCode { get; set; }
        public string RegionCode { get; set; }
    }

    public class DataTablesReportRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<string> FilterStatus { get; set; }
        public Search Search { get; set; }
        public List<Column> Columns { get; set; }
        public List<Order> Order { get; set; }
        public List<string> Venue { get; set; }
        public List<string> Activity { get; set; }
        public string ExamCode { get; set; }

        public string Subject { get; set; }
        public string PaperCode { get; set; }
        public string RegionCode { get; set; }
    }

    public class DataTablesRequest2
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search Search { get; set; }
        public List<Column> Columns { get; set; }
        public List<Order> Order { get; set; }

        // Your existing filter properties
        public string FilterStatus { get; set; }
        public string Venue { get; set; }
        // ... other filter properties
    }

    public class Search
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public Search Search { get; set; }
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; } // "asc" or "desc"
    }
}
