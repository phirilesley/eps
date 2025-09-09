using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Helpers;
using Oracle.ManagedDataAccess;
using System.Data.SqlClient;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using ExaminerPaymentSystem.Controllers.Examiners;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMarksCapturedRepository _marksCapturedRepository;

        private readonly IPaperMarkingRateRepository _paperMarkingRate;
        private readonly ICategoryRateRepository _categoryMarkingRate;
        private readonly IExaminerRepository _examinerRepository;
        private readonly IBrailleTranscriptionRateRepository _railleTranscriptionRate;
        private readonly ITranscribersRepository _transcribersRepository;
        private readonly IConfiguration _configuration;

        public TransactionRepository(ApplicationDbContext context, IPaperMarkingRateRepository paperMarkingRate,
            ICategoryRateRepository categoryMarkingRate, IMarksCapturedRepository marksCapturedRepository,
            IExaminerRepository examinerRepository, IBrailleTranscriptionRateRepository railleTranscriptionRate,
            ITranscribersRepository transcribersRepository, IConfiguration configuration)
        {
            _context = context;

            _categoryMarkingRate = categoryMarkingRate;
            _paperMarkingRate = paperMarkingRate;
            _marksCapturedRepository = marksCapturedRepository;
            _examinerRepository = examinerRepository;
            _railleTranscriptionRate = railleTranscriptionRate;
            _transcribersRepository = transcribersRepository;
            _configuration = configuration;
        }

        public decimal CalculatePaymentTotal(CalculationValuesDto calculationValues)
        {
            return 0;
        }

        public async Task<List<ExaminerScriptsMarked>> CheckExaminerTransactions(string examinerCode,
            string subjectCode, string paperCode, string searchBmsCode)
        {
            try
            {
                var subKey = examinerCode + subjectCode + paperCode;
                var examiners = await _context.EXAMINER_TRANSACTIONS
                    .Where(e => e.EMS_SUBKEY.StartsWith(subKey))
                    .ToListAsync();

                List<ExaminerScriptsMarked> finalData = new List<ExaminerScriptsMarked>();


                foreach (var item in examiners)
                {
                    var checkRegister = await _context.ExaminerRegister.FirstOrDefaultAsync(a =>
                        a.EMS_SUBKEY == item.EMS_SUBKEY && a.ExaminerCode == item.EMS_EXAMINER_CODE &&
                        a.IDNumber == item.EMS_NATIONAL_ID);

                    if (checkRegister != null && checkRegister.Status == "Present")
                    {
                        finalData.Add(item);
                    }
                }


                return finalData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        public async Task<IEnumerable<ExaminerScriptsMarked>> GetTeamByBms(string examCode, string subjectCode,
            string paperCode, string searchBmsCode)
        {
            var examiners = await _context.EXAMINER_TRANSACTIONS
                .Where(em => em.EMS_SUB_SUB_ID == examCode + subjectCode
                             && (em.EMS_EXM_SUPERORD == searchBmsCode /*|| em.EMS_EXAMINER_NUMBER == searchBmsCode*/)
                             && em.EMS_PAPER_CODE == paperCode && em.RegisterStatus == "Present" && em.EMS_ACTIVITY == "BEM")
                .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                .GroupJoin(
                    _context.EXM_EXAMINER_MASTER,
                    em => em.EMS_NATIONAL_ID,
                    examiner => examiner.EMS_NATIONAL_ID,
                    (em, examinerGroup) => new { em, examinerGroup })
                .SelectMany(
                    x => x.examinerGroup.DefaultIfEmpty(),
                    (x, examiner) => new ExaminerScriptsMarked
                    {
                        EMS_EXAMINER_NUMBER = x.em.EMS_EXAMINER_NUMBER,
                        AttendanceStatus = x.em.AttendanceStatus,
                        EMS_EXM_SUPERORD = x.em.EMS_EXM_SUPERORD,
                        EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                        EMS_SUBKEY = x.em.EMS_SUBKEY,
                        EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                        EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                        EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                        SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                        EMS_CAPTURINGROLE = x.em.EMS_CAPTURINGROLE,
                        IsPresent = x.em.IsPresent,
                        EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                        EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX,
                        EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                        Examiner = examiner // This will be null if no matching Examiner is found
                    })
                .ToListAsync();

            return examiners;
        }


        public async Task<ExaminerScriptsMarked> CheckExaminerTransactionsT(string subkey, string idNumber,
            string examinerCode)
        {
            try
            {
                var examiner = await _context.EXAMINER_TRANSACTIONS
                    .FirstOrDefaultAsync(e =>
                        e.EMS_SUBKEY == subkey && e.EMS_NATIONAL_ID == idNumber && e.EMS_EXAMINER_CODE == examinerCode);
                return examiner;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<ExaminerScriptsMarked> CheckExaminerTransactions(string subkey, string idNumber,
            string examinerCode, string subid, string papercode)
        {
            try
            {
                var examiner = await _context.EXAMINER_TRANSACTIONS
                    .FirstOrDefaultAsync(e =>
                        e.EMS_SUBKEY == subkey && e.EMS_NATIONAL_ID == idNumber &&
                        e.EMS_EXAMINER_CODE == examinerCode && e.EMS_SUB_SUB_ID == subid &&
                        e.EMS_PAPER_CODE == papercode);
                return examiner;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task<List<ExaminerScriptsMarked>> FixmissingButPresent(List<ExaminerScriptsMarked> examiners,
            List<RegisterDto> presentExaminers, string subKey)
        {
            int? BMSScriptMarked = 0;
            int? DPMSScpiptMarked = 0;
            int? ExaminerScriptMarked = 0;
            int? RPMSSCrpitMarked = 0;
            int? PMSScriptMarked = 0;

            foreach (var item in examiners)
            {
                var examinerRecord = await _context.EXM_EXAMINER_MASTER.FirstOrDefaultAsync(a =>
                    a.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID && a.EMS_SUBKEY == item.EMS_SUBKEY &&
                    a.EMS_EXAMINER_CODE == item.EMS_EXAMINER_CODE);

                if (examinerRecord != null)
                {
                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                    {
                        DPMSScpiptMarked = item.SCRIPTS_MARKED;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "E")
                    {
                        ExaminerScriptMarked = item.SCRIPTS_MARKED;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                    {
                        PMSScriptMarked = item.SCRIPTS_MARKED;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                    {
                        RPMSSCrpitMarked = item.SCRIPTS_MARKED;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                    {
                        BMSScriptMarked = item.SCRIPTS_MARKED;
                    }
                }
            }

            var presentExaminersNotInTransactions = presentExaminers
                .Where(pe =>
                    !examiners.Any(e => e.EMS_EXAMINER_CODE == pe.ExaminerCode && e.EMS_SUBKEY == pe.EMS_SUBKEY))
                .ToList();

            List<TempDA> tempDAs = new List<TempDA>();
            foreach (var item in presentExaminersNotInTransactions)
            {
                var examinerRecord = await _context.EXM_EXAMINER_MASTER.FirstOrDefaultAsync(a =>
                    a.EMS_NATIONAL_ID == item.IDNumber && a.EMS_SUBKEY == item.EMS_SUBKEY &&
                    a.EMS_EXAMINER_CODE == item.ExaminerCode);


                if (examinerRecord != null)
                {
                    var data = new TempDA()
                    {
                        examinercode = item.ExaminerCode,
                        idnumber = item.IDNumber,
                        subkey = item.EMS_SUBKEY,
                        subsubid = examinerRecord.EMS_SUB_SUB_ID,
                        papercode = examinerRecord.EMS_PAPER_CODE,
                        Role = examinerRecord.EMS_ECT_EXAMINER_CAT_CODE,
                    };

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "DPMS")
                    {
                        data.scrpitMarket = DPMSScpiptMarked;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "E")
                    {
                        data.scrpitMarket = ExaminerScriptMarked;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "PMS")
                    {
                        data.scrpitMarket = PMSScriptMarked;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "RPMS")
                    {
                        data.scrpitMarket = RPMSSCrpitMarked;
                    }

                    if (examinerRecord.EMS_ECT_EXAMINER_CAT_CODE == "BMS")
                    {
                        data.scrpitMarket = BMSScriptMarked;
                    }

                    tempDAs.Add(data);
                }
            }

            List<ExaminerScriptsMarked> scriptsMarkeds = new List<ExaminerScriptsMarked>();
            foreach (var data in tempDAs)
            {
                var trans = new ExaminerScriptsMarked()
                {
                    EMS_EXAMINER_CODE = data.examinercode,
                    EMS_SUBKEY = data.subkey,
                    SCRIPTS_MARKED = data.scrpitMarket,
                    EMS_NATIONAL_ID = data.idnumber,
                    EMS_SUB_SUB_ID = data.subsubid,
                    EMS_PAPER_CODE = data.papercode,
                    EMS_COMPILED_STATUS = "Compiled",
                    EMS_APPROVED_STATUS = "Approved",
                    EMS_CENTRE_SUPERVISOR_STATUS = "Approved",
                    EMS_CERTIFIED_STATUS = "Certified",
                };
                scriptsMarkeds.Add(trans);
            }

            foreach (var script in scriptsMarkeds)
            {
                var existingRecord = await _context.EXAMINER_TRANSACTIONS
                    .FirstOrDefaultAsync(e =>
                        e.EMS_EXAMINER_CODE == script.EMS_EXAMINER_CODE && e.EMS_SUBKEY == script.EMS_SUBKEY);

                if (existingRecord != null)
                {
                    existingRecord.SCRIPTS_MARKED = script.SCRIPTS_MARKED;
                    existingRecord.EMS_COMPILED_STATUS = script.EMS_COMPILED_STATUS;
                    existingRecord.EMS_APPROVED_STATUS = script.EMS_APPROVED_STATUS;
                    existingRecord.EMS_CENTRE_SUPERVISOR_STATUS = script.EMS_CENTRE_SUPERVISOR_STATUS;
                    existingRecord.EMS_CERTIFIED_STATUS = script.EMS_CERTIFIED_STATUS;
                    _context.EXAMINER_TRANSACTIONS.Update(existingRecord);
                }
                else
                {
                    await _context.EXAMINER_TRANSACTIONS.AddAsync(script);
                }
            }

            await _context.SaveChangesAsync();

            return scriptsMarkeds;
        }


        public async Task<List<ExaminerScriptsMarked>> GetComponentExaminer(string examcode, string subject,
            string papercode, string regionCode)
        {
            try
            {
                List<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();

                var subKey = examcode + subject + papercode + "BEM";

                if (!string.IsNullOrEmpty(regionCode))
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS
                        .Where(s => s.EMS_SUB_SUB_ID == examcode + subject && s.EMS_PAPER_CODE == papercode &&
                                    s.EMS_ACTIVITY == "BEM" && s.EMS_SUBKEY.StartsWith(subKey) &&
                                    s.RegisterStatus == "Present" && s.IsPresent && s.SCRIPTS_MARKED > 0 &&
                                    s.EMS_MARKING_REG_CODE == regionCode)
                        .Include(a => a.Examiner)
                        .ToListAsync();
                }
                else
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS
                        .Where(s => s.EMS_SUB_SUB_ID == examcode + subject && s.EMS_PAPER_CODE == papercode &&
                                    s.EMS_ACTIVITY == "BEM" && s.EMS_SUBKEY.StartsWith(subKey) &&
                                    s.RegisterStatus == "Present" && s.IsPresent && s.SCRIPTS_MARKED > 0)
                        .Include(a => a.Examiner)
                        .ToListAsync();
                }

              
                return examiners;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception here or return null, depending on your requirements
                throw;
            }
        }


        public async Task<List<ExaminerScriptsMarked>> GetLevelComponentExaminer(string examcode)
        {
            try
            {
                List<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();

                var subKey = examcode;


                examiners = await _context.EXAMINER_TRANSACTIONS
                    .Where(s => s.EMS_SUB_SUB_ID.StartsWith(examcode) && s.EMS_ACTIVITY == "BEM" &&
                                s.EMS_SUBKEY.StartsWith(subKey) && s.RegisterStatus == "Present" && s.IsPresent &&
                                s.SCRIPTS_MARKED > 0)
                    .Include(a => a.Examiner)
                    .ToListAsync();


                return examiners;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception here or return null, depending on your requirements
                throw;
            }
        }

        public async Task<List<ExaminerScriptsMarked>> GetMasterExaminers(List<string> examCodes)
        {
            try
            {
                List<ExaminerScriptsMarked> examiners = await _context.EXAMINER_TRANSACTIONS
                    .Where(s => examCodes.Any(code => s.EMS_SUB_SUB_ID.StartsWith(code))
                                && examCodes.Any(code => s.EMS_SUBKEY.StartsWith(code))
                                && s.EMS_ACTIVITY == "BEM"
                                && s.RegisterStatus == "Present"
                                && s.IsPresent
                                && s.SCRIPTS_MARKED > 0)
                    .Include(a => a.Examiner)
                    .ToListAsync();

                return examiners;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Optionally rethrow the exception for higher-level handling
            }
        }


        public async Task<List<ExaminerScriptsMarked>> GetAllTransactionExaminer()
        {
            try
            {
                var examiners = await _context.EXAMINER_TRANSACTIONS
                    .ToListAsync();

                return examiners;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception here or return null, depending on your requirements
                throw;
            }
        }


        public async Task<OperationResult> PerformOperationsOnExceptMultipleChoicePapers(string examcode, string subject,
            string papercode, string capturingRate, string? regioncode, ApplicationUser applicationUser)
        {
            var checkpapermarkingrate =
                                    await _paperMarkingRate.GetPaperMarkingRate(subject, papercode, examcode);

            if(checkpapermarkingrate == null)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Paper Marking Rate Missing"
                };
            }

            if(checkpapermarkingrate.PPR_MARKING_RATE == null)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Paper Marking Rate Missing."
                };
            }

            var mergedExaminerList = new List<ExaminerScriptsMarkedReportModel>();
            var examiners = await GetComponentExaminer(examcode, subject, papercode, regioncode);


            var notApprovedExaminers = examiners
    .Where(item => item.EMS_COMPILED_STATUS != "Compiled"
                || item.EMS_APPROVED_STATUS != "Approved"
                || item.EMS_CERTIFIED_STATUS != "Certified"
                || item.EMS_CENTRE_SUPERVISOR_STATUS != "Approved")
    .ToList();

            if (notApprovedExaminers.Any())
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Some examiners are not yet Approved."
                };
            }

            mergedExaminerList = examiners
 .Where(item => item.EMS_COMPILED_STATUS == "Compiled"
                && item.EMS_APPROVED_STATUS == "Approved"
                && item.EMS_CERTIFIED_STATUS == "Certified"
                && item.EMS_CENTRE_SUPERVISOR_STATUS == "Approved")
 .Select(item => new ExaminerScriptsMarkedReportModel
 {
     EMS_NATIONAL_ID = item.EMS_NATIONAL_ID,
     EMS_SUBKEY = item.EMS_SUBKEY,
     SCRIPTS_MARKED = item.SCRIPTS_MARKED,
     EMS_EXAMINER_CODE = item.EMS_EXAMINER_CODE,
     EMS_MARKING_REG_CODE = item.EMS_MARKING_REG_CODE,
     EMS_ECT_EXAMINER_CAT_CODE = item.EMS_ECT_EXAMINER_CAT_CODE
 })
 .ToList();

            if (!examiners.Any())
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "No examiners to calculate or they are not yet Approved."
                };
            }

            //foreach (var item in examiners)
            //{
            //    if (item.EMS_COMPILED_STATUS == "Compiled" && item.EMS_APPROVED_STATUS == "Approved" && item.EMS_CERTIFIED_STATUS == "Certified" && item.EMS_CENTRE_SUPERVISOR_STATUS == "Approved")
            //    {


            //        var newReportModel = new ExaminerScriptsMarkedReportModel()
            //        {
            //            EMS_NATIONAL_ID = item.EMS_NATIONAL_ID,
            //            EMS_SUBKEY = item.EMS_SUBKEY,
            //            SCRIPTS_MARKED = item.SCRIPTS_MARKED,
            //            EMS_EXAMINER_CODE = item.EMS_EXAMINER_CODE,
            //            EMS_MARKING_REG_CODE = item.EMS_MARKING_REG_CODE,
            //            EMS_ECT_EXAMINER_CAT_CODE = item.EMS_ECT_EXAMINER_CAT_CODE,

            //        };
            //        mergedExaminerList.Add(newReportModel);
            //    }

            //}




            decimal totalexaminers = 0;
            decimal totalscripts = 0;
            decimal averagenumberofscriptsmarked = 0;
            decimal capturingFee = 0;


            if (subject.StartsWith("7"))
            {
                var examinersdata = mergedExaminerList
                    .Where(c => c.EMS_MARKING_REG_CODE == regioncode && c.EMS_ECT_EXAMINER_CAT_CODE == "E").ToList();
                totalexaminers = examinersdata.Count();
                totalscripts = 0;
                foreach (var item in examinersdata)
                {
                    totalscripts += item.SCRIPTS_MARKED?.ToString() != null
                        ? int.Parse(item.SCRIPTS_MARKED.ToString())
                        : 0;
                }

                averagenumberofscriptsmarked = CalculateAverageScripts(totalscripts, totalexaminers);
                var marksCaptured = await _marksCapturedRepository.GetMarkCapturedGrade7List(examcode, subject);

                if(marksCaptured == null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Marks captured not yet in the system."
                    };
                }

               

                if (marksCaptured == null || !marksCaptured.Any())
                {
                    capturingFee = 0;
                }
                else
                {
                    decimal datanumber = 0;
                    foreach (var item in marksCaptured)
                    {
                        datanumber += item.TotalScriptsCaptured;
                    }

                    var examinersincomponent =
                        mergedExaminerList.Where(c => c.EMS_MARKING_REG_CODE == regioncode).ToList();
                    var totalexaminerscomponent = examinersincomponent.Count();
                    decimal S = datanumber;
                    decimal E = totalexaminerscomponent;
                    decimal r = decimal.Parse(capturingRate);

                    capturingFee = 2 * S / E * r;
                    capturingFee = Math.Round(capturingFee, 2);
                }
            }
            else
            {
                var examinersdata = mergedExaminerList.Where(c => c.EMS_ECT_EXAMINER_CAT_CODE == "E").ToList();
                totalexaminers = examinersdata.Count();
                totalscripts = 0;

                foreach (var item in examinersdata)
                {
                    totalscripts += int.Parse(item.SCRIPTS_MARKED?.ToString() ?? "0");
                }

                averagenumberofscriptsmarked = CalculateAverageScripts(totalscripts, totalexaminers);

                var marksCaptured =
                    await _marksCapturedRepository.GetMarkCapturedByParameters(examcode, subject, papercode);

                if (marksCaptured == null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Marks captured not yet in the system."
                    };
                }

                if (marksCaptured.TotalScriptsCaptured == null || marksCaptured.TotalScriptsCaptured == 0) {

                    if (marksCaptured.AccountsTotalScriptCaptured == null || marksCaptured.TotalScriptsCaptured == 0) {
                        return new OperationResult
                        {
                            Success = false,
                            Message = "Marks captured not yet in the system."
                        };

                    }

                
                }

                if (marksCaptured == null)
                {
                    capturingFee = 0;
                }
                else
                {
                    var examinersincomponent = mergedExaminerList.ToList();
                    var totalexaminerscomponent = examinersincomponent.Count();
                    decimal S = 0;
                    if (marksCaptured.TotalScriptsCaptured != null)
                    {
                        S = marksCaptured.TotalScriptsCaptured;
                    }
                    else
                    {
                        S = marksCaptured.AccountsTotalScriptCaptured;
                    }

                    decimal E = totalexaminerscomponent;
                    decimal r = decimal.Parse(capturingRate);

                    capturingFee = 2 * S / E * r;
                    capturingFee = Math.Round(capturingFee, 2);
                }
            }

            List<ExaminerScriptsMarked> notCalculated = new List<ExaminerScriptsMarked>();

            // Grouping examiners by EMS_ECT_EXAMINER_CAT_CODE
            var groupedExaminers = mergedExaminerList.GroupBy(e => e.EMS_ECT_EXAMINER_CAT_CODE);

            // Perform operations on each group
            foreach (var group in groupedExaminers)
            {
                switch (group.Key)
                {
                    case "PMS":

                        List<ExaminerScriptsMarked> pmsscriptsMarkeds = new List<ExaminerScriptsMarked>();
                        foreach (var pmsexaminer in group)
                        {
                            if (pmsexaminer.SCRIPTS_MARKED > 0)
                            {
                                var papermarkingrate =
                                    await _paperMarkingRate.GetPaperMarkingRate(subject, papercode, examcode);


                                var categorymarkingrate =
                                    await _categoryMarkingRate.GetCategoryMarkingRate(papermarkingrate.PPR_EXAM_TYPE,
                                        pmsexaminer.EMS_ECT_EXAMINER_CAT_CODE);
                                decimal responsibility = 0;
                                decimal Xp = 0;
                                decimal Yp = 0;
                                decimal modfees = 0;
                                //var natReportAllowance = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                //var coordination = decimal.Parse(categorymarkingrate.COORD_FEES);
                                if (categorymarkingrate != null)
                                {
                                    responsibility = decimal.Parse(categorymarkingrate.MOD_FEES) +
                                                     decimal.Parse(categorymarkingrate.SUPER_FEES);
                                    Xp = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                    Yp = decimal.Parse(categorymarkingrate.COORD_FEES);
                                    modfees = decimal.Parse(categorymarkingrate.MOD_FEES);
                                }

                                var scriptmarked = pmsexaminer.SCRIPTS_MARKED;
                                var scriptrate = decimal.Parse(papermarkingrate.PPR_MARKING_RATE);


                                decimal Qp = 0;
                                if (averagenumberofscriptsmarked == 0)
                                {
                                    Qp = 0;
                                }
                                else
                                {
                                    Qp = responsibility * averagenumberofscriptsmarked * scriptrate;
                                }

                                var Nr = scriptmarked * scriptrate;

                                var total = Xp + Yp + Qp + Nr;

                                var examinerscipt = new ExaminerScriptsMarked()
                                {
                                    EMS_NATIONAL_ID = pmsexaminer.EMS_NATIONAL_ID,
                                    EMS_EXAMINER_CODE = pmsexaminer.EMS_EXAMINER_CODE,
                                    EMS_SUBKEY = pmsexaminer.EMS_SUBKEY,
                                    SCRIPT_RATE = scriptrate,
                                    COORDINATION_FEES = 0,
                                    MODERATION_FEES = modfees,
                                    RESPONSIBILITY_FEES = Qp + Xp + Yp,
                                    SCRIPTS_MARKED = pmsexaminer.SCRIPTS_MARKED,
                                    CAPTURING_FEES = capturingFee,
                                    GRAND_TOTAL = total,
                                    PaidStatus = "Not Paid"
                                };

                                pmsscriptsMarkeds.Add(examinerscipt);
                            }
                        }

                        await UpdateExaminerScriptMarked(pmsscriptsMarkeds, applicationUser);
                        break;
                    case "RPMS":
                        List<ExaminerScriptsMarked> rpmsscriptsMarkeds = new List<ExaminerScriptsMarked>();
                        foreach (var rpmsexaminer in group)
                        {
                            if (rpmsexaminer.SCRIPTS_MARKED > 0)
                            {
                                var papermarkingrate =
                                    await _paperMarkingRate.GetPaperMarkingRate(subject, papercode, examcode);


                                var categorymarkingrate =
                                    await _categoryMarkingRate.GetCategoryMarkingRate(papermarkingrate.PPR_EXAM_TYPE,
                                        rpmsexaminer.EMS_ECT_EXAMINER_CAT_CODE);

                                decimal responsibility = 0;
                                decimal Xp = 0;
                                decimal Yp = 0;
                                decimal modfees = 0;
                                //var natReportAllowance = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                //var coordination = decimal.Parse(categorymarkingrate.COORD_FEES);
                                if (categorymarkingrate != null)
                                {
                                    responsibility = decimal.Parse(categorymarkingrate.MOD_FEES) +
                                                     decimal.Parse(categorymarkingrate.SUPER_FEES);
                                    Xp = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                    Yp = decimal.Parse(categorymarkingrate.COORD_FEES);
                                    modfees = decimal.Parse(categorymarkingrate.MOD_FEES);
                                }

                                var scriptmarked = rpmsexaminer.SCRIPTS_MARKED;
                                var scriptrate = decimal.Parse(papermarkingrate.PPR_MARKING_RATE);


                                decimal Qp = 0;
                                if (averagenumberofscriptsmarked == 0)
                                {
                                    Qp = 0;
                                }
                                else
                                {
                                    Qp = responsibility * averagenumberofscriptsmarked * scriptrate;
                                }

                                var Nr = scriptmarked * scriptrate;

                                var total = Xp + Yp + Qp + Nr;

                                var examinerscipt = new ExaminerScriptsMarked()
                                {
                                    EMS_NATIONAL_ID = rpmsexaminer.EMS_NATIONAL_ID,
                                    EMS_EXAMINER_CODE = rpmsexaminer.EMS_EXAMINER_CODE,
                                    EMS_SUBKEY = rpmsexaminer.EMS_SUBKEY,
                                    SCRIPT_RATE = scriptrate,
                                    COORDINATION_FEES = 0,
                                    MODERATION_FEES = modfees,
                                    RESPONSIBILITY_FEES = Qp + Xp + Yp,
                                    SCRIPTS_MARKED = rpmsexaminer.SCRIPTS_MARKED,
                                    CAPTURING_FEES = capturingFee,
                                    GRAND_TOTAL = total,
                                    PaidStatus = "Not Paid"
                                };

                                rpmsscriptsMarkeds.Add(examinerscipt);
                            }
                        }

                        await UpdateExaminerScriptMarked(rpmsscriptsMarkeds, applicationUser);
                        break;
                    case "DPMS":

                        List<ExaminerScriptsMarked> dpmsscriptsMarkeds = new List<ExaminerScriptsMarked>();
                        foreach (var dpmsexaminer in group)
                        {
                            if (dpmsexaminer.SCRIPTS_MARKED > 0)
                            {
                                var papermarkingrate =
                                    await _paperMarkingRate.GetPaperMarkingRate(subject, papercode, examcode);


                                var categorymarkingrate =
                                    await _categoryMarkingRate.GetCategoryMarkingRate(papermarkingrate.PPR_EXAM_TYPE,
                                        dpmsexaminer.EMS_ECT_EXAMINER_CAT_CODE);

                                decimal responsibility = 0;
                                decimal Xp = 0;
                                decimal Yp = 0;
                                decimal modfees = 0;
                                //var natReportAllowance = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                //var coordination = decimal.Parse(categorymarkingrate.COORD_FEES);
                                if (categorymarkingrate != null)
                                {
                                    responsibility = decimal.Parse(categorymarkingrate.MOD_FEES) +
                                                     decimal.Parse(categorymarkingrate.SUPER_FEES);
                                    Xp = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                    Yp = decimal.Parse(categorymarkingrate.COORD_FEES);
                                    modfees = decimal.Parse(categorymarkingrate.MOD_FEES);
                                }

                                var scriptmarked = dpmsexaminer.SCRIPTS_MARKED;
                                var scriptrate = decimal.Parse(papermarkingrate.PPR_MARKING_RATE);


                                decimal Qp = 0;
                                if (averagenumberofscriptsmarked == 0)
                                {
                                    Qp = 0;
                                }
                                else
                                {
                                    Qp = responsibility * averagenumberofscriptsmarked * scriptrate;
                                }

                                var Nr = scriptmarked * scriptrate;

                                var total = Xp + Yp + Qp + Nr;

                                var examinerscipt = new ExaminerScriptsMarked()
                                {
                                    EMS_NATIONAL_ID = dpmsexaminer.EMS_NATIONAL_ID,
                                    EMS_EXAMINER_CODE = dpmsexaminer.EMS_EXAMINER_CODE,
                                    EMS_SUBKEY = dpmsexaminer.EMS_SUBKEY,
                                    SCRIPT_RATE = scriptrate,
                                    COORDINATION_FEES = 0,
                                    MODERATION_FEES = modfees,
                                    RESPONSIBILITY_FEES = Qp + Xp + Yp,
                                    SCRIPTS_MARKED = dpmsexaminer.SCRIPTS_MARKED,
                                    CAPTURING_FEES = capturingFee,
                                    GRAND_TOTAL = total,
                                    PaidStatus = "Not Paid"
                                };

                                dpmsscriptsMarkeds.Add(examinerscipt);
                            }
                        }

                        await UpdateExaminerScriptMarked(dpmsscriptsMarkeds, applicationUser);
                        break;
                    case "BMS":

                        List<ExaminerScriptsMarked> bmsscriptsMarkeds = new List<ExaminerScriptsMarked>();
                        foreach (var bmsexaminer in group)
                        {
                            if (bmsexaminer.SCRIPTS_MARKED > 0)
                            {
                                var papermarkingrate =
                                    await _paperMarkingRate.GetPaperMarkingRate(subject, papercode, examcode);


                                var categorymarkingrate =
                                    await _categoryMarkingRate.GetCategoryMarkingRate(papermarkingrate.PPR_EXAM_TYPE,
                                        bmsexaminer.EMS_ECT_EXAMINER_CAT_CODE);

                                decimal responsibility = 0;
                                decimal Xp = 0;
                                decimal Yp = 0;
                                decimal modfees = 0;
                                //var natReportAllowance = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                //var coordination = decimal.Parse(categorymarkingrate.COORD_FEES);
                                if (categorymarkingrate != null)
                                {
                                    responsibility = decimal.Parse(categorymarkingrate.MOD_FEES) +
                                                     decimal.Parse(categorymarkingrate.SUPER_FEES);
                                    Xp = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                    Yp = decimal.Parse(categorymarkingrate.COORD_FEES);
                                    modfees = decimal.Parse(categorymarkingrate.MOD_FEES);
                                }

                                var scriptmarked = bmsexaminer.SCRIPTS_MARKED;
                                var scriptrate = decimal.Parse(papermarkingrate.PPR_MARKING_RATE);


                                decimal Qp = 0;
                                if (averagenumberofscriptsmarked == 0)
                                {
                                    Qp = 0;
                                }
                                else
                                {
                                    Qp = responsibility * averagenumberofscriptsmarked * scriptrate;
                                }

                                var Nr = scriptmarked * scriptrate;


                                var total = Xp + Yp + Qp + Nr;

                                var examinerscipt = new ExaminerScriptsMarked()
                                {
                                    EMS_NATIONAL_ID = bmsexaminer.EMS_NATIONAL_ID,
                                    EMS_EXAMINER_CODE = bmsexaminer.EMS_EXAMINER_CODE,
                                    EMS_SUBKEY = bmsexaminer.EMS_SUBKEY,
                                    SCRIPT_RATE = scriptrate,
                                    COORDINATION_FEES = 0,
                                    MODERATION_FEES = modfees,
                                    RESPONSIBILITY_FEES = Qp + Xp + Yp,
                                    SCRIPTS_MARKED = bmsexaminer.SCRIPTS_MARKED,
                                    CAPTURING_FEES = capturingFee,
                                    GRAND_TOTAL = total,
                                    PaidStatus = "Not Paid"
                                    
                                };

                                bmsscriptsMarkeds.Add(examinerscipt);
                            }
                        }

                        await UpdateExaminerScriptMarked(bmsscriptsMarkeds, applicationUser);
                        break;
                    case "E":

                        List<ExaminerScriptsMarked> scriptsMarkeds = new List<ExaminerScriptsMarked>();
                        foreach (var examiner in group)
                        {
                            if (examiner.SCRIPTS_MARKED > 0)
                            {
                                var papermarkingrate =
                                    await _paperMarkingRate.GetPaperMarkingRate(subject, papercode, examcode);


                                var categorymarkingrate =
                                    await _categoryMarkingRate.GetCategoryMarkingRate(papermarkingrate.PPR_EXAM_TYPE,
                                        examiner.EMS_ECT_EXAMINER_CAT_CODE);

                                var scriptmarked = examiner.SCRIPTS_MARKED;
                                var scriptrate = decimal.Parse(papermarkingrate.PPR_MARKING_RATE);

                                decimal responsibility = 0;
                                decimal Xp = 0;
                                decimal Yp = 0;
                                decimal modfees = 0;
                                //var natReportAllowance = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                //var coordination = decimal.Parse(categorymarkingrate.COORD_FEES);
                                if (categorymarkingrate != null)
                                {
                                    responsibility = decimal.Parse(categorymarkingrate.MOD_FEES) +
                                                     decimal.Parse(categorymarkingrate.SUPER_FEES);
                                    Xp = decimal.Parse(categorymarkingrate.NAT_REP_ALLOWANCE);
                                    Yp = decimal.Parse(categorymarkingrate.COORD_FEES) * scriptrate;
                                    modfees = decimal.Parse(categorymarkingrate.MOD_FEES);
                                }


                                var Qp = responsibility * averagenumberofscriptsmarked * scriptrate;
                                var Nr = scriptmarked * scriptrate;

                                var total = Yp + Nr;

                                var examinerscipt = new ExaminerScriptsMarked()
                                {
                                    EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                                    EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                                    EMS_SUBKEY = examiner.EMS_SUBKEY,
                                    SCRIPT_RATE = scriptrate,
                                    COORDINATION_FEES = Yp,
                                    SCRIPTS_MARKED = examiner.SCRIPTS_MARKED,
                                    CAPTURING_FEES = capturingFee,
                                    GRAND_TOTAL = total,
                                    PaidStatus = "Not Paid"
                                };

                                scriptsMarkeds.Add(examinerscipt);
                            }
                        }

                        await UpdateExaminerScriptMarked(scriptsMarkeds, applicationUser);
                        break;
                   
                    default:

                        foreach (var examiner in group)
                        {

                        }

                        break;
                }
            }

            return new OperationResult
            {
                Success = true,
                Message = "Success"
            };
        }


        public decimal CalculateAverageScripts(decimal totalScripts, decimal totalExaminers)
        {
            decimal averageNumberOfScriptsMarked = 0;

            if (totalExaminers != 0)
            {
                averageNumberOfScriptsMarked = totalScripts / totalExaminers;
                averageNumberOfScriptsMarked = Math.Round(averageNumberOfScriptsMarked, 2);
            }

            return averageNumberOfScriptsMarked;
        }


        public async Task PerformOperationsOnMultipleChoicePapers(string examcode, string subject, string papercode,
            string bmscode, string? regioncode)
        {
            try
            {
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task InsertScriptMarked(List<ExaminerScriptsMarked> examiners, string userid)
        {
            try
            {
                foreach (var item in examiners)
                {
                    var existingEntity = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(e =>
                        e.EMS_SUBKEY == item.EMS_SUBKEY &&
                        e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID && e.EMS_SUB_SUB_ID == item.EMS_SUB_SUB_ID &&
                        e.EMS_PAPER_CODE == item.EMS_PAPER_CODE && e.EMS_ACTIVITY == "BEM"
                    );


                    if (existingEntity != null)
                    {
                        existingEntity.SCRIPTS_MARKED = item.SCRIPTS_MARKED;
                        existingEntity.EMS_APPROVED_BY = item.EMS_APPROVED_BY;
                        existingEntity.EMS_APPROVED_DATE = item.EMS_APPROVED_DATE;
                        existingEntity.EMS_APPROVED_STATUS = item.EMS_APPROVED_STATUS;
                        existingEntity.EMS_COMPILED_BY = item.EMS_COMPILED_BY;
                        existingEntity.EMS_COMPILED_STATUS = item.EMS_COMPILED_STATUS;
                        existingEntity.EMS_COMPILED_DATE = item.EMS_COMPILED_DATE;
                        existingEntity.EMS_CERTIFIED_BY = item.EMS_CERTIFIED_BY;
                        existingEntity.EMS_CERTIFIED_STATUS = item.EMS_CERTIFIED_STATUS;
                        existingEntity.EMS_CERTIFIED_DATE = item.EMS_CERTIFIED_DATE;
                        existingEntity.EMS_CENTRE_SUPERVISOR_DATE = item.EMS_CENTRE_SUPERVISOR_DATE;
                        existingEntity.EMS_CENTRE_SUPERVISOR_NAME = item.EMS_CENTRE_SUPERVISOR_NAME;
                        existingEntity.EMS_CENTRE_SUPERVISOR_STATUS = item.EMS_CENTRE_SUPERVISOR_STATUS;
                        existingEntity.EMS_SUB_SUB_ID = item.EMS_SUB_SUB_ID;
                        existingEntity.EMS_PAPER_CODE = item.EMS_PAPER_CODE;
                        existingEntity.EMS_CAPTURINGROLE = item.EMS_CAPTURINGROLE;
                       existingEntity.EMS_PERFORMANCE_INDEX = item.EMS_PERFORMANCE_INDEX;

                        _context.EXAMINER_TRANSACTIONS.Update(existingEntity);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateExaminerScriptMarked(List<ExaminerScriptsMarked> examiners,
            ApplicationUser applicationUser)
        {
            foreach (var item in examiners)
            {
                var existingEntity = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(e =>
                    e.EMS_SUBKEY == item.EMS_SUBKEY &&
                    e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID
                );

                if (existingEntity != null)
                {
                    existingEntity.CAPTURING_FEES = item.CAPTURING_FEES;
                    existingEntity.COORDINATION_FEES = item.COORDINATION_FEES;
                    existingEntity.RESPONSIBILITY_FEES = item.RESPONSIBILITY_FEES;
                    existingEntity.MODERATION_FEES = item.MODERATION_FEES;
                    existingEntity.GRAND_TOTAL = item.GRAND_TOTAL;
                    existingEntity.SCRIPT_RATE = item.SCRIPT_RATE;
                    existingEntity.CAPTURING_FEES = item.CAPTURING_FEES;
                    existingEntity.PaidStatus = item.PaidStatus;    

                    _context.EXAMINER_TRANSACTIONS.Update(existingEntity);
                }
                else
                {
                }
            }

            await _context.SaveChangesAsync(applicationUser.Id);
        }

        public async Task DeleteTransaction(string nationalId, string subkey, ApplicationUser applicationUser)
        {
            var tands = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM.FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == nationalId && e.EMS_SUBKEY == subkey);
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

        public async Task<IEnumerable<ExaminerScriptsMarked>> CheckExaminerInTransactions(string examCode,
            string subjectCode, string paperCode, string regionCode)
        {
            try
            {
                IEnumerable<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();
                if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS
                        .Where(em => em.EMS_SUB_SUB_ID == subjectCode
                                     && em.SCRIPTS_MARKED > 0 && em.EMS_MARKING_REG_CODE == regionCode
                                     && em.EMS_PAPER_CODE == paperCode && em.RegisterStatus == "Present" &&
                                     em.EMS_ACTIVITY == "BEM" && em.IsPresent)
                        .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                        .GroupJoin(
                            _context.EXM_EXAMINER_MASTER,
                            em => em.EMS_NATIONAL_ID,
                            examiner => examiner.EMS_NATIONAL_ID,
                            (em, examinerGroup) => new { em, examinerGroup })
                        .SelectMany(
                            x => x.examinerGroup.DefaultIfEmpty(),
                            (x, examiner) => new ExaminerScriptsMarked
                            {
                                EMS_EXAMINER_NUMBER = x.em.EMS_EXAMINER_NUMBER,
                                EMS_EXM_SUPERORD = x.em.EMS_EXM_SUPERORD,
                                EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                                EMS_SUBKEY = x.em.EMS_SUBKEY,
                                EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                                EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                                EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                                SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                                EMS_CAPTURINGROLE = x.em.EMS_CAPTURINGROLE,
                                EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                                Examiner = examiner,
                                EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                                EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                                EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                                EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                                IsPresent = x.em.IsPresent,
                                EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                                EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                            })
                        .ToListAsync();
                }
                else
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS
                        .Where(em => em.EMS_SUB_SUB_ID == subjectCode
                                     && em.SCRIPTS_MARKED > 0
                                     && em.EMS_PAPER_CODE == paperCode && em.RegisterStatus == "Present" &&
                                     em.EMS_ACTIVITY == "BEM" && em.IsPresent)
                        .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                        .GroupJoin(
                            _context.EXM_EXAMINER_MASTER,
                            em => em.EMS_NATIONAL_ID,
                            examiner => examiner.EMS_NATIONAL_ID,
                            (em, examinerGroup) => new { em, examinerGroup })
                        .SelectMany(
                            x => x.examinerGroup.DefaultIfEmpty(),
                            (x, examiner) => new ExaminerScriptsMarked
                            {
                                EMS_EXAMINER_NUMBER = x.em.EMS_EXAMINER_NUMBER,
                                EMS_EXM_SUPERORD = x.em.EMS_EXM_SUPERORD,
                                EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                                EMS_SUBKEY = x.em.EMS_SUBKEY,
                                EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                                EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                                EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                                SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                                EMS_CAPTURINGROLE = x.em.EMS_CAPTURINGROLE,
                                EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                                Examiner = examiner,
                                EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                                EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                                EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                                EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                                IsPresent = x.em.IsPresent,
                                EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                                EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                            })
                        .ToListAsync();
                }

                return examiners;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<IEnumerable<ExaminerScriptsMarked>> CheckPresentExaminersInTransactions(string examCode,
            string subjectCode, string paperCode, string regionCode)
        {
            try
            {
                IEnumerable<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();
                if (!string.IsNullOrEmpty(regionCode) && subjectCode.StartsWith("7"))
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS
                        .Where(em => em.EMS_SUB_SUB_ID == subjectCode && em.EMS_MARKING_REG_CODE == regionCode
                                                                      && em.EMS_PAPER_CODE == paperCode &&
                                                                      em.RegisterStatus == "Present" &&
                                                                      em.EMS_ACTIVITY == "BEM" && em.IsPresent)
                        .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                        .GroupJoin(
                            _context.EXM_EXAMINER_MASTER,
                            em => em.EMS_NATIONAL_ID,
                            examiner => examiner.EMS_NATIONAL_ID,
                            (em, examinerGroup) => new { em, examinerGroup })
                        .SelectMany(
                            x => x.examinerGroup.DefaultIfEmpty(),
                            (x, examiner) => new ExaminerScriptsMarked
                            {
                                EMS_EXAMINER_NUMBER = x.em.EMS_EXAMINER_NUMBER,
                                EMS_EXM_SUPERORD = x.em.EMS_EXM_SUPERORD,
                                EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                                EMS_SUBKEY = x.em.EMS_SUBKEY,
                                EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                                EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                                EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                                SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                                EMS_CAPTURINGROLE = x.em.EMS_CAPTURINGROLE,
                                EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                                Examiner = examiner,
                                EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                                EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                                EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                                EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                                IsPresent = x.em.IsPresent,
                                EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                                EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                            })
                        .ToListAsync();
                }
                else
                {
                    examiners = await _context.EXAMINER_TRANSACTIONS
                        .Where(em => em.EMS_SUB_SUB_ID == subjectCode
                                     && em.EMS_PAPER_CODE == paperCode && em.RegisterStatus == "Present" &&
                                     em.EMS_ACTIVITY == "BEM" && em.IsPresent)
                        .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                        .GroupJoin(
                            _context.EXM_EXAMINER_MASTER,
                            em => em.EMS_NATIONAL_ID,
                            examiner => examiner.EMS_NATIONAL_ID,
                            (em, examinerGroup) => new { em, examinerGroup })
                        .SelectMany(
                            x => x.examinerGroup.DefaultIfEmpty(),
                            (x, examiner) => new ExaminerScriptsMarked
                            {
                                EMS_EXAMINER_NUMBER = x.em.EMS_EXAMINER_NUMBER,
                                EMS_EXM_SUPERORD = x.em.EMS_EXM_SUPERORD,
                                EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                                EMS_SUBKEY = x.em.EMS_SUBKEY,
                                EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                                EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                                EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                                SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                                EMS_CAPTURINGROLE = x.em.EMS_CAPTURINGROLE,
                                EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                                Examiner = examiner,
                                EMS_COMPILED_STATUS = x.em.EMS_COMPILED_STATUS,
                                EMS_APPROVED_STATUS = x.em.EMS_APPROVED_STATUS,
                                EMS_CENTRE_SUPERVISOR_STATUS = x.em.EMS_CENTRE_SUPERVISOR_STATUS,
                                EMS_CERTIFIED_STATUS = x.em.EMS_CERTIFIED_STATUS,
                                IsPresent = x.em.IsPresent,
                                EMS_ACTIVITY = x.em.EMS_ACTIVITY,
                                EMS_PERFORMANCE_INDEX = x.em.EMS_PERFORMANCE_INDEX
                            })
                        .ToListAsync();
                }

                return examiners;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        public async Task<IEnumerable<ExaminerScriptsMarked>> CheckTranscribersInTransactions()
        {
            // Implement the logic to query EXAMINER_TRANSACTIONS table

            try
            {
                var trans = await _context.EXM_EXAMINER_MASTER
                    .Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT")
                    .ToListAsync();
                var Data = new List<ExaminerScriptsMarked>();
                foreach (var item in trans)
                {
                    var examiner = await _context.EXAMINER_TRANSACTIONS
                        .FirstOrDefaultAsync(e =>
                            e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID && e.EMS_SUBKEY == item.EMS_SUBKEY &&
                            e.EMS_EXAMINER_CODE == item.EMS_EXAMINER_CODE && e.SCRIPTS_MARKED != null);
                    if (examiner != null)
                    {
                        Data.Add(examiner);
                    }
                }


                return Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<bool> ApproveTranscribers(IEnumerable<string> userRoles, ApplicationUser currentUser)
        {
            try
            {
                var examinersdata = await _context.EXM_EXAMINER_MASTER
                    .Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT")
                    .ToListAsync();
                List<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();
                foreach (var item in examinersdata)
                {
                    var data = await _context.EXAMINER_TRANSACTIONS
                        .FirstOrDefaultAsync(e =>
                            e.EMS_SUBKEY == item.EMS_SUBKEY && e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID &&
                            e.SCRIPTS_MARKED != null);
                    if (data != null)
                    {
                        examiners.Add(data);
                    }
                }


                if (examiners == null || !examiners.Any())
                {
                    return false;
                }

                // Perform the approval logic
                foreach (var examiner in examiners)
                {
                    var checkregister = await _context.ExaminerRegister.FirstOrDefaultAsync(e =>
                        e.ExaminerCode == examiner.EMS_EXAMINER_CODE && e.EMS_SUBKEY == examiner.EMS_SUBKEY &&
                        e.IDNumber == examiner.EMS_NATIONAL_ID);

                    if (checkregister != null && checkregister.Status == "Present")
                    {
                        if (userRoles.Contains("OfficerSpecialNeeds"))
                        {
                            examiner.EMS_CERTIFIED_STATUS = "Certified";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_CERTIFIED_BY = currentUser.UserName;
                            examiner.EMS_CERTIFIED_DATE = DateTime.Now.ToString();
                        }
                        else if (userRoles.Contains("CentreSupervisor"))
                        {
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Approved";
                            examiner.EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName;
                            examiner.EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString();
                        }

                        _context.EXAMINER_TRANSACTIONS.Update(examiner);
                    }
                }

                // Save the changes to the database
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // You can use a logging framework like NLog, Serilog, etc.
                return false;
            }
        }

        public async Task<bool> ApproveExaminers(string examCode, string subjectCode, string paperCode,
            IEnumerable<string> userRoles, ApplicationUser currentUser)
        {
            try
            {
                var examinersdata = await _examinerRepository.GetExaminersByParameters(subjectCode, paperCode);
                List<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();
                foreach (var item in examinersdata)
                {
                    var data = await _context.EXAMINER_TRANSACTIONS
                        .FirstOrDefaultAsync(e =>
                            e.EMS_SUBKEY == item.EMS_SUBKEY && e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID &&
                            e.SCRIPTS_MARKED != null);
                    if (data != null)
                    {
                        examiners.Add(data);
                    }
                }

                var key = examCode + subjectCode + paperCode;

                var data2 = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUBKEY.StartsWith(key)).ToListAsync();

                foreach (var item in data2)
                {
                    if (!examiners.Any(e => e.EMS_SUBKEY == item.EMS_SUBKEY))
                    {
                        examiners.Add(item); // Add directly if the type matches
                    }
                }


                if (examiners == null || !examiners.Any())
                {
                    return false;
                }

                // Perform the approval logic
                foreach (var examiner in examiners)
                {
                    var checkregister = await _context.ExaminerRegister.FirstOrDefaultAsync(e =>
                        e.ExaminerCode == examiner.EMS_EXAMINER_CODE && e.EMS_SUBKEY == examiner.EMS_SUBKEY &&
                        e.IDNumber == examiner.EMS_NATIONAL_ID);

                    if (checkregister != null && checkregister.Status == "Present")
                    {
                        if (userRoles.Contains("PMS"))
                        {
                            examiner.EMS_APPROVED_STATUS = "Approved";
                            examiner.EMS_CERTIFIED_STATUS = "Pending";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_APPROVED_BY = currentUser.UserName;
                            examiner.EMS_APPROVED_DATE = DateTime.Now.ToString();
                        }
                        else if (userRoles.Contains("DPMS"))
                        {
                            examiner.EMS_APPROVED_STATUS = "Approved";
                            examiner.EMS_CERTIFIED_STATUS = "Pending";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_APPROVED_BY = currentUser.UserName;
                            examiner.EMS_APPROVED_DATE = DateTime.Now.ToString();
                        }
                        else if (userRoles.Contains("RPMS"))
                        {
                            examiner.EMS_APPROVED_STATUS = "Approved";
                            examiner.EMS_CERTIFIED_STATUS = "Pending";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_APPROVED_BY = currentUser.UserName;
                            examiner.EMS_APPROVED_DATE = DateTime.Now.ToString();
                        }
                        else if (userRoles.Contains("SubjectManager") || userRoles.Contains("OfficerSpecialNeeds"))
                        {
                            examiner.EMS_CERTIFIED_STATUS = "Certified";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_CERTIFIED_BY = currentUser.UserName;
                            examiner.EMS_CERTIFIED_DATE = DateTime.Now.ToString();
                        }
                        else if (userRoles.Contains("CentreSupervisor"))
                        {
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Approved";
                            examiner.EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName;
                            examiner.EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString();
                        }

                        _context.EXAMINER_TRANSACTIONS.Update(examiner);
                    }
                }

                // Save the changes to the database
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // You can use a logging framework like NLog, Serilog, etc.
                return false;
            }
        }


        public async Task<bool> ApproveAllTrans(IEnumerable<string> userRoles, ApplicationUser currentUser)
        {
            try
            {
                var examinersdata = await _transcribersRepository.GetAllTranscribers();
                List<ExaminerScriptsMarked> examiners = new List<ExaminerScriptsMarked>();
                foreach (var item in examinersdata)
                {
                    var data = await _context.EXAMINER_TRANSACTIONS
                        .FirstOrDefaultAsync(e =>
                            e.EMS_SUBKEY == item.EMS_SUBKEY && e.EMS_NATIONAL_ID == item.EMS_NATIONAL_ID &&
                            e.SCRIPTS_MARKED != null);
                    if (data != null)
                    {
                        examiners.Add(data);
                    }
                }


                if (examiners == null || !examiners.Any())
                {
                    return false;
                }

                // Perform the approval logic
                foreach (var examiner in examiners)
                {
                    var checkregister = await _context.ExaminerRegister.FirstOrDefaultAsync(e =>
                        e.ExaminerCode == examiner.EMS_EXAMINER_CODE && e.EMS_SUBKEY == examiner.EMS_SUBKEY &&
                        e.IDNumber == examiner.EMS_NATIONAL_ID);

                    if (checkregister != null && checkregister.Status == "Present")
                    {
                        if (userRoles.Contains("SubjectManager") || userRoles.Contains("OfficerSpecialNeeds"))
                        {
                            examiner.EMS_CERTIFIED_STATUS = "Certified";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_CERTIFIED_BY = currentUser.UserName;
                            examiner.EMS_CERTIFIED_DATE = DateTime.Now.ToString();
                        }
                        else if (userRoles.Contains("CentreSupervisor"))
                        {
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Approved";
                            examiner.EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName;
                            examiner.EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString();
                        }

                        _context.EXAMINER_TRANSACTIONS.Update(examiner);
                    }
                }

                // Save the changes to the database
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // You can use a logging framework like NLog, Serilog, etc.
                return false;
            }
        }

        public async Task<EntriesData> GetEntries(string examcode, string subject, string papercode, string bms)
        {
            var data = await _context.EntriesData.FirstOrDefaultAsync(e =>
                e.ExamCode == examcode && e.Subject == subject && e.PaperCode == papercode && e.BMS == bms);
            return data;
        }

        public async Task<EntriesData> GetEntriesTrascribers()
        {
            var data = await _context.EntriesData.FirstOrDefaultAsync();
            return data;
        }

        public async Task UpdatePresent(UpdateTranscactionPresenceRequest request, string userId)
        {
            var transaction = await _context.EXAMINER_TRANSACTIONS
                .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == request.idNumber && e.EMS_SUBKEY == request.SubKey);
            

       
                transaction.IsPresent = request.IsPresent ? true : false;
            transaction.IsPresentBy = userId;
            transaction.IsPresentDate = DateTime.Now.ToString();
                _context.EXAMINER_TRANSACTIONS.Update(transaction);
            

            await _context.SaveChangesAsync(userId);
        }

        public async Task<IEnumerable<ExaminerScriptsMarked>> GetExaminerTransctionsAsync(string idNumber)
        {
            return await _context.EXAMINER_TRANSACTIONS
                .Where(e => e.EMS_NATIONAL_ID == idNumber)
                .ToListAsync();
        }

        public async Task<ExaminerScriptsMarked> GetExaminerTransactionAsync(string idNumber, string examCode,
            string subjectCode, string paperCode, string activity)
        {
            try
            {
                var examiner = await _context.EXAMINER_TRANSACTIONS
                    .FirstOrDefaultAsync(e =>
                        e.EMS_NATIONAL_ID == idNumber && e.EMS_SUB_SUB_ID == examCode + subjectCode &&
                        e.EMS_PAPER_CODE == paperCode && e.EMS_ACTIVITY == activity);
                return examiner;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<ExaminerScriptsMarked>> CheckExaminersPresentButNoScriptMarked(string examcode,
            string subject, string papercode, string regionCode)
        {
            List<ExaminerScriptsMarked> examinersInTransctions = new List<ExaminerScriptsMarked>();
            if (!string.IsNullOrEmpty(regionCode) && subject.StartsWith("7"))
            {
                examinersInTransctions = await _context.EXAMINER_TRANSACTIONS.Where(a =>
                    a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == papercode &&
                    a.EMS_MARKING_REG_CODE == regionCode && a.SCRIPTS_MARKED <= 0 && a.RegisterStatus == "Present" &&
                    a.EMS_ACTIVITY == "BEM" && a.IsPresent).ToListAsync();
            }
            else
            {
                examinersInTransctions = await _context.EXAMINER_TRANSACTIONS.Where(a =>
                    a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == papercode && a.RegisterStatus == "Present" &&
                    a.EMS_ACTIVITY == "BEM" && a.SCRIPTS_MARKED <= 0 && a.IsPresent).ToListAsync();
            }

            return examinersInTransctions;
        }

        public async Task<List<ExaminerScriptsMarked>> CheckExaminersPresentButNotApproved(string examcode,
            string subject, string papercode, string regionCode)
        {
            List<ExaminerScriptsMarked> examinersInTransctions = new List<ExaminerScriptsMarked>();
            if (!string.IsNullOrEmpty(regionCode) && subject.StartsWith("7"))
            {
                examinersInTransctions = await _context.EXAMINER_TRANSACTIONS.Where(a =>
                    a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == papercode &&
                    a.EMS_MARKING_REG_CODE == regionCode && a.SCRIPTS_MARKED > 0 && a.RegisterStatus == "Present" &&
                    a.EMS_ACTIVITY == "BEM" && a.IsPresent && (a.EMS_COMPILED_STATUS == "Pending" || a.EMS_APPROVED_STATUS == "Pending")).ToListAsync();
            }
            else
            {
                examinersInTransctions = await _context.EXAMINER_TRANSACTIONS.Where(a =>
                    a.EMS_SUB_SUB_ID == subject && a.EMS_PAPER_CODE == papercode && a.RegisterStatus == "Present" &&
                    a.EMS_ACTIVITY == "BEM" && a.SCRIPTS_MARKED > 0 && a.IsPresent && (a.EMS_COMPILED_STATUS == "Pending" || a.EMS_APPROVED_STATUS == "Pending")).ToListAsync();
            }

            return examinersInTransctions;
        }

        public async Task<OperationResult> ApproveExaminers(ApprovalRequestModel request, IEnumerable<string> userRoles,
            ApplicationUser currentUser)
        {
            try
            {
                if (userRoles.Contains("PMS"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUB_SUB_ID == request.SubjectCode &&
                            a.EMS_PAPER_CODE == request.PaperCode && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_APPROVED_STATUS = "Approved";
                            examiner.EMS_CERTIFIED_STATUS = "Pending";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_APPROVED_BY = currentUser.UserName;
                            examiner.EMS_APPROVED_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }
                else if (userRoles.Contains("DPMS"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUB_SUB_ID == request.SubjectCode &&
                            a.EMS_PAPER_CODE == request.PaperCode && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_APPROVED_STATUS = "Approved";
                            examiner.EMS_CERTIFIED_STATUS = "Pending";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_APPROVED_BY = currentUser.UserName;
                            examiner.EMS_APPROVED_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }
                else if (userRoles.Contains("RPMS"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUB_SUB_ID == request.SubjectCode &&
                            a.EMS_PAPER_CODE == request.PaperCode && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_APPROVED_STATUS = "Approved";
                            examiner.EMS_CERTIFIED_STATUS = "Pending";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_APPROVED_BY = currentUser.UserName;
                            examiner.EMS_APPROVED_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }
                else if (userRoles.Contains("SubjectManager") || userRoles.Contains("OfficerSpecialNeeds"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUB_SUB_ID == request.SubjectCode &&
                            a.EMS_PAPER_CODE == request.PaperCode && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_CERTIFIED_STATUS = "Certified";
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Pending";
                            examiner.EMS_CERTIFIED_BY = currentUser.UserName;
                            examiner.EMS_CERTIFIED_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }
                else if (userRoles.Contains("CentreSupervisor"))
                {
                    foreach (var item in request.Examiners)
                    {
                        var examiner = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a =>
                            a.EMS_NATIONAL_ID == item.IdNumber && a.EMS_SUB_SUB_ID == request.SubjectCode &&
                            a.EMS_PAPER_CODE == request.PaperCode && a.EMS_SUBKEY == item.SubKey && a.IsPresent);

                        if (examiner != null)
                        {
                            examiner.EMS_CENTRE_SUPERVISOR_STATUS = "Approved";
                            examiner.EMS_CENTRE_SUPERVISOR_NAME = currentUser.UserName;
                            examiner.EMS_CENTRE_SUPERVISOR_DATE = DateTime.Now.ToString();
                            _context.EXAMINER_TRANSACTIONS.Update(examiner);
                        }
                    }
                }


                // Save the changes to the database
                await _context.SaveChangesAsync(currentUser.Id);

                return new OperationResult { Success = true, Message = "Examiner approved successfully" };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<OperationResult> AddNewExaminerToTransaction(ExaminerScriptsMarked examiner, string userId)
        {
            if (examiner == null)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Examiner Transaction details are required."
                };
            }


            var checkIdNumber = await _context.EXAMINER_TRANSACTIONS
                .AnyAsync(c =>
                    c.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && c.EMS_SUBKEY == examiner.EMS_SUBKEY &&
                    c.EMS_SUB_SUB_ID == examiner.EMS_SUB_SUB_ID && c.EMS_PAPER_CODE == examiner.EMS_PAPER_CODE &&
                    c.EMS_ACTIVITY == examiner.EMS_ACTIVITY);

            if (checkIdNumber)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "An examiner with the same National ID and   already exists."
                };
            }


            // Add the new examiner and save changes
            _context.EXAMINER_TRANSACTIONS.Add(examiner);
            await _context.SaveChangesAsync(userId);

            return new OperationResult
            {
                Success = true,
                Message = "Examiner Transaction details are required."
            };
        }

        public async Task<IEnumerable<ExaminerScriptsMarked>> GetExaminersFromTransaction(string examCode,
            string subjectCode, string paperCode)
        {
            var examiners = await _context.EXAMINER_TRANSACTIONS
                .Where(em => em.EMS_SUB_SUB_ID == examCode + subjectCode
                             && em.EMS_PAPER_CODE == paperCode)
                .OrderBy(em => em.EMS_EXAMINER_NUMBER)
                .GroupJoin(
                    _context.EXM_EXAMINER_MASTER,
                    em => em.EMS_NATIONAL_ID,
                    examiner => examiner.EMS_NATIONAL_ID,
                    (em, examinerGroup) => new { em, examinerGroup })
                .SelectMany(
                    x => x.examinerGroup.DefaultIfEmpty(),
                    (x, examiner) => new ExaminerScriptsMarked
                    {
                        EMS_EXAMINER_NUMBER = x.em.EMS_EXAMINER_NUMBER,
                        EMS_EXM_SUPERORD = x.em.EMS_EXM_SUPERORD,
                        EMS_PAPER_CODE = x.em.EMS_PAPER_CODE,
                        EMS_SUBKEY = x.em.EMS_SUBKEY,
                        EMS_SUB_SUB_ID = x.em.EMS_SUB_SUB_ID,
                        EMS_EXAMINER_CODE = x.em.EMS_EXAMINER_CODE,
                        EMS_NATIONAL_ID = x.em.EMS_NATIONAL_ID,
                        SCRIPTS_MARKED = x.em.SCRIPTS_MARKED,
                        EMS_CAPTURINGROLE = x.em.EMS_CAPTURINGROLE,
                        IsPresent = x.em.IsPresent,
                        EMS_ECT_EXAMINER_CAT_CODE = x.em.EMS_ECT_EXAMINER_CAT_CODE,
                        Examiner = examiner // This will be null if no matching Examiner is found
                    })
                .ToListAsync();

            return examiners;
        }

        public async Task<OperationResult> UpdateExaminerToTransaction(ExaminerScriptsMarked examiner, string userId)
        {
            try
            {
                var existing = await _context.EXAMINER_TRANSACTIONS
                    .FirstOrDefaultAsync(c =>
                        c.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && c.EMS_SUBKEY == examiner.EMS_SUBKEY &&
                        c.EMS_SUB_SUB_ID == examiner.EMS_SUB_SUB_ID && c.EMS_PAPER_CODE == examiner.EMS_PAPER_CODE &&
                        c.EMS_ACTIVITY == examiner.EMS_ACTIVITY);

                if (existing != null)
                {
                    if (examiner.EMS_EXAMINER_NUMBER != null)
                    {
                        existing.EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER;
                    }

                    if (examiner.EMS_ECT_EXAMINER_CAT_CODE != null)
                    {
                        existing.EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE;
                    }

                    if (examiner.EMS_MARKING_REG_CODE != null)
                    {
                        existing.EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE;
                    }

                    if (examiner.EMS_EXM_SUPERORD != null)
                    {
                        existing.EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD;
                    }

                    _context.EXAMINER_TRANSACTIONS.Update(existing);
                    await _context.SaveChangesAsync(userId);
                }

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


        public async Task<IEnumerable<ExaminerScriptsMarked>> GetInvitedExaminers(string examCode, string activity)
        {
            return await _context.EXAMINER_TRANSACTIONS
                .Where(a => a.EMS_ACTIVITY == activity && a.EMS_SUBKEY.StartsWith(examCode))
                .Include(a => a.Examiner)
                .ToListAsync();
        }

        public async Task<bool> ExportExaminerTransactionToOracle(string examCode, string subjectCode, string paperCode,
            string? regionCode)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText =
                            "SELECT ET.EMS_EXAMINER_CODE, ET.EMS_NATIONAL_ID, ET.EMS_ECT_EXAMINER_CAT_CODE, \r\n   " +
                            "ET.EMS_SUB_SUB_ID,EM.EMS_REGION_CODE, ET.EMS_PAPER_CODE, ET.EMS_MARKING_REG_CODE, ET.EMS_EXAMINER_NUMBER, ET.EMS_EXM_SUPERORD, \r\n   " +
                            "ET.EMS_SUBKEY,EM.EMS_LEVEL_OF_EXAM_MARKED,\r\n 'A' as EMS_STATUS, EM.EMS_COMMENTS,\r\n " +
                            "CASE WHEN EM.EMS_PERFORMANCE_INDEX IS NULL THEN 'A' ELSE EM.EMS_PERFORMANCE_INDEX END as EMS_PERFORMANCE_INDEX ,\r\n " +
                            "'Y' as EMS_SELECTED_FLAG, EMS_ACTIVITY\r\n " +
                            "FROM dbo.EXAMINER_TRANSACTIONS ET \r\n" +
                            "INNER JOIN dbo.EXM_EXAMINER_MASTER EM on EM.EMS_NATIONAL_ID = ET.EMS_NATIONAL_ID\r\n" +
                            "WHERE ET.EMS_SUB_SUB_ID = @SubjectId AND ET.EMS_PAPER_CODE = @PaperCode AND ET.EMS_ACTIVITY = @activity";
                        cmd.Parameters.Add(new SqlParameter("SubjectId", subjectCode));
                        cmd.Parameters.Add(new SqlParameter("PaperCode", paperCode));
                        cmd.Parameters.Add(new SqlParameter("activity", "BEM"));
                        if (regionCode is not null)
                        {
                            cmd.CommandText += " AND EM.EMS_REGION_CODE = @RegionCode";
                            cmd.Parameters.Add(new SqlParameter("RegionCode", regionCode));
                        }

                        con.Open();
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        dataTable.Load(reader);
                    }
                    catch (Exception )
                    {
                        return false;
                    }
                }
            }

            

            using (OracleConnection oracle =
                   new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
            {
                oracle.Open();
                try
                {
                    using (var cmd = new OracleCommand(
                               "DELETE FROM ZIMSYS.EXM_EXAMINER_TRANSACTIONS WHERE EMS_SUB_SUB_ID || EMS_PAPER_CODE = :17",
                               oracle))
                    {
                        cmd.Parameters.Add(":17", OracleDbType.Varchar2, subjectCode + paperCode,
                            ParameterDirection.Input);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = oracle;
                        cmd.CommandText = @"
                            INSERT INTO ZIMSYS.EXM_EXAMINER_TRANSACTIONS 
                            (EMS_EXAMINER_CODE, EMS_NATIONAL_ID, EMS_ECT_EXAMINER_CAT_CODE, EMS_SUB_SUB_ID, EMS_REGION_CODE,
                             EMS_PAPER_CODE, EMS_MARKING_REG_CODE, EMS_EXAMINER_NUMBER, EMS_EXM_SUPERORD, EMS_SUBKEY,
                             EMS_LEVEL_OF_EXAM_MARKED, EMS_STATUS, EMS_COMMENTS, EMS_PERFORMANCE_INDEX, EMS_SELECTED_FLAG, EMS_ACTIVITY)
                            VALUES (:1, :2, :3, :4, :5, :6, :7, :8, :9, :10, :11, :12, :13, :14, :15, :16)";

                        // Convert DataTable columns to arrays
                        cmd.ArrayBindCount = dataTable.Rows.Count;
                        cmd.Parameters.Add(":1", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_EXAMINER_CODE"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":2", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_NATIONAL_ID"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":3", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_ECT_EXAMINER_CAT_CODE"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":4", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_SUB_SUB_ID"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":5", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_REGION_CODE"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":6", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_PAPER_CODE"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":7", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_MARKING_REG_CODE"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":8", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_EXAMINER_NUMBER"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":9", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_EXM_SUPERORD"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":10", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_SUBKEY"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":11", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_LEVEL_OF_EXAM_MARKED"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":12", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_STATUS"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":13", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_COMMENTS"]).ToArray(), ParameterDirection.Input);
                        cmd.Parameters.Add(":14", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_PERFORMANCE_INDEX"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":15", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_SELECTED_FLAG"]).ToArray(),
                            ParameterDirection.Input);
                        cmd.Parameters.Add(":16", OracleDbType.Varchar2,
                            dataTable.AsEnumerable().Select(r => r["EMS_ACTIVITY"]).ToArray(), ParameterDirection.Input);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }

            return true;
        }

        public async Task<bool> ExportRefCatToOracle(string examCode, string subjectCode, string paperCode,
            string? regionCode)
        {
            DataTable dataTable = new DataTable();
            string subsubId = subjectCode + paperCode;

            try
            {
                // Step 1: Fetch from SQL Server
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = @" SELECT CTP_PPR_SUB_PAPER_CODE, 
                                                CTP_ECT_CAT_CODE, 
                                                CTP_MAX_SCRIPTS, 
                                                CTP_REGION_CODE 
                                         FROM dbo.REF_CAT_PAPER
                                         WHERE CTP_PPR_SUB_PAPER_CODE = @subsubId";

                    cmd.Parameters.AddWithValue("@subsubId", subsubId);

                    if (!string.IsNullOrEmpty(regionCode))
                    {
                        cmd.CommandText += " AND CTP_REGION_CODE = @regionCode";
                        cmd.Parameters.AddWithValue("@regionCode", regionCode);
                    }

                    await con.OpenAsync();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    dataTable.Load(reader);
                }

                if (dataTable.Rows.Count == 0)
                {
                    return true; // Nothing to export
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SQL Error: " + ex.Message);
                return false;
            }


            try
            {
                using (OracleConnection oracle = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
                {
                    await oracle.OpenAsync();

                    using(var cmd = new OracleCommand(
                               "DELETE FROM ZIMSYS.REF_CAT_PAPER WHERE CTP_PPR_SUB_PAPER_CODE = :1",
                               oracle))
                    {
                        cmd.Parameters.Add(":1", OracleDbType.Varchar2, subjectCode + paperCode, ParameterDirection.Input);
                        cmd.ExecuteNonQuery();
                    }

                    string insertSql = @"
                                        INSERT INTO ZIMSYS.REF_CAT_PAPER 
                                        (CTP_PPR_SUB_PAPER_CODE, CTP_ECT_CAT_CODE, CTP_MAX_SCRIPTS, CTP_REGION_CODE)
                                        VALUES (:CTP_PPR_SUB_PAPER_CODE, :CTP_ECT_CAT_CODE, :CTP_MAX_SCRIPTS, :CTP_REGION_CODE)";

                    using (var command = new OracleCommand(insertSql, oracle))
                    {
                        command.BindByName = true;

                        int rowCount = dataTable.Rows.Count;
                        command.ArrayBindCount = rowCount;

                        // Convert DataTable columns to arrays
                        var subPaperCodes = dataTable.AsEnumerable()
                            .Select(r => r["CTP_PPR_SUB_PAPER_CODE"]?.ToString()).ToArray();

                        var catCodes = dataTable.AsEnumerable()
                            .Select(r => r["CTP_ECT_CAT_CODE"]?.ToString()).ToArray();

                        var maxScripts = dataTable.AsEnumerable()
                            .Select(r => Convert.ToInt32(r["CTP_MAX_SCRIPTS"])).ToArray();

                        var regionCodes = dataTable.AsEnumerable()
                            .Select(r => r["CTP_REGION_CODE"]?.ToString()).ToArray();

                        // Bind arrays
                        command.Parameters.Add(":CTP_PPR_SUB_PAPER_CODE", OracleDbType.Varchar2).Value = subPaperCodes;
                        command.Parameters.Add(":CTP_ECT_CAT_CODE", OracleDbType.Varchar2).Value = catCodes;
                        command.Parameters.Add(":CTP_MAX_SCRIPTS", OracleDbType.Int32).Value = maxScripts;
                        command.Parameters.Add(":CTP_REGION_CODE", OracleDbType.Varchar2).Value = regionCodes;

                        await command.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oracle Check Error: " + ex.Message);
                return false;
            }
            return true;
        }

        public async Task<bool> ImportExaminerTransactionFromOracle(string examCode, string subjectCode,
            string paperCode, string? regionCode)
        {
            DataTable dataTable = new DataTable();

            using (OracleConnection oracle =
                   new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
            {
                oracle.Open();
                using (OracleCommand cmd = oracle.CreateCommand())
                {
                    cmd.CommandText =
                        $"SELECT EMS_SUBKEY, EMS_EXAMINER_CODE, EMS_SUB_SUB_ID, EMS_PAPER_CODE, EMS_EXAMINER_NUMBER,EMS_MARKING_REG_CODE, EMS_ECT_EXAMINER_CAT_CODE, EMS_EXM_SUPERORD,EMS_NATIONAL_ID, 0 as IsPresent, EMS_ACTIVITY\r\n " +
                        $"FROM ZIMSYS.EXM_EXAMINER_TRANSACTIONS {(regionCode is null ? "WHERE EMS_SUB_SUB_ID = :SubjectId AND EMS_PAPER_CODE=:PaperCode" : "WHERE EMS_SUB_SUB_ID = :SubjectId AND EMS_PAPER_CODE=:PaperCode AND EMS_REGION_CODE = :RegionCode ")}";
                    cmd.Parameters.Add("SubjectId", subjectCode);
                    cmd.Parameters.Add("PaperCode", paperCode);
                    if (regionCode is not null)
                    {
                        cmd.Parameters.Add("RegionCode", regionCode);
                    }

                    OracleDataReader dataReader = await cmd.ExecuteReaderAsync();
                    dataTable.Load(dataReader);
                }
            }

            using (SqlConnection oracle = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                oracle.Open();

                using (SqlBulkCopy bc = new SqlBulkCopy(_configuration.GetConnectionString("DefaultConnection")))
                {
                    bc.DestinationTableName = "EXAMINER_TRANSACTIONS";
                    bc.ColumnMappings.Add("EMS_EXAMINER_CODE", "EMS_EXAMINER_CODE");
                    bc.ColumnMappings.Add("EMS_NATIONAL_ID", "EMS_NATIONAL_ID");
                    bc.ColumnMappings.Add("EMS_ECT_EXAMINER_CAT_CODE", "EMS_ECT_EXAMINER_CAT_CODE");
                    bc.ColumnMappings.Add("EMS_SUB_SUB_ID", "EMS_SUB_SUB_ID");
                    bc.ColumnMappings.Add("EMS_PAPER_CODE", "EMS_PAPER_CODE");
                    bc.ColumnMappings.Add("EMS_MARKING_REG_CODE", "EMS_MARKING_REG_CODE");
                    bc.ColumnMappings.Add("EMS_EXAMINER_NUMBER", "EMS_EXAMINER_NUMBER");
                    bc.ColumnMappings.Add("EMS_EXM_SUPERORD", "EMS_EXM_SUPERORD");
                    bc.ColumnMappings.Add("EMS_SUBKEY", "EMS_SUBKEY");
                    bc.ColumnMappings.Add("IsPresent", "IsPresent");
                    bc.ColumnMappings.Add("EMS_ACTIVITY", "EMS_ACTIVITY");
                    try
                    {

                        bc.WriteToServer(dataTable);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task DeleteTranscation(string nationalId, ApplicationUser applicationUser)
        {
            var trans = await _context.EXAMINER_TRANSACTIONS.Where(e => e.EMS_NATIONAL_ID == nationalId).ToListAsync();
            if (trans.Any())
            {

                _context.EXAMINER_TRANSACTIONS.RemoveRange(trans);
                await _context.SaveChangesAsync(applicationUser.Id);
            }

               
            }
        

        public class TempDA
        {
            public int? scrpitMarket { get; set; }
            public string? Role { get; set; }
            public string? examinercode { get; set; }

            public string? idnumber { get; set; }

            public string? subsubid { get; set; }


            public string? papercode { get; set; }

            public string? subkey { get; set; }
        }
    }
}