using Dapper;
using DocumentFormat.OpenXml.Office.Word;
using ExaminerPaymentSystem.Controllers;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Crypto.Modes;
using System.Data;
using System.Web.Mvc;


namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class ExaminerRepository : IExaminerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ExaminerRepository(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }


        public async Task<List<ExaminerDto>> GetAbsentExaminersByCategoryAsync(string categoryCode, string examCode,string subSubId, string paperCode,string activity, string regionCode)
        {
            var result = new List<ExaminerDto>();

            if (!string.IsNullOrEmpty(regionCode) && subSubId.StartsWith("7"))
            {

                result = await _context.EXAMINER_TRANSACTIONS
     .Join(_context.EXM_EXAMINER_MASTER,
         eem => eem.EMS_NATIONAL_ID,  // Key from EXAMINER_TRANSACTIONS
         master => master.EMS_NATIONAL_ID,  // Key from EXM_EXAMINER_MASTER
         (eem, master) => new { eem, master })  // Combine data
     .Where(x => x.eem.EMS_SUB_SUB_ID == examCode + subSubId &&
                 x.eem.EMS_PAPER_CODE == paperCode &&
                 x.eem.EMS_ACTIVITY == activity &&
                 x.eem.EMS_MARKING_REG_CODE == regionCode && x.eem.RegisterStatus == "Absent" && x.eem.EMS_ECT_EXAMINER_CAT_CODE == categoryCode)
     .Select(x => new ExaminerDto
     {
         ExaminerNumber = x.eem.EMS_EXAMINER_NUMBER,
         ExaminerName = x.master.EMS_LAST_NAME + " " + x.master.EMS_EXAMINER_NAME, // Master table data
         Superord = x.eem.EMS_EXM_SUPERORD,
         ExaminerCategoryCode = x.eem.EMS_ECT_EXAMINER_CAT_CODE,
         Status = x.eem.RegisterStatus,
         SubSubId = x.eem.EMS_SUB_SUB_ID,
     })
     .ToListAsync();


            }
            else
            {
                result = await _context.EXAMINER_TRANSACTIONS
     .Join(_context.EXM_EXAMINER_MASTER,
         eem => eem.EMS_NATIONAL_ID,  // Key from EXAMINER_TRANSACTIONS
         master => master.EMS_NATIONAL_ID,  // Key from EXM_EXAMINER_MASTER
         (eem, master) => new { eem, master })  // Combine data
     .Where(x => x.eem.EMS_SUB_SUB_ID == examCode + subSubId &&
                 x.eem.EMS_PAPER_CODE == paperCode &&
                 x.eem.EMS_ACTIVITY == activity && x.eem.RegisterStatus == "Absent" && x.eem.EMS_ECT_EXAMINER_CAT_CODE == categoryCode)
     .Select(x => new ExaminerDto
     {
         ExaminerNumber = x.eem.EMS_EXAMINER_NUMBER,
         ExaminerName = x.master.EMS_LAST_NAME + " " + x.master.EMS_EXAMINER_NAME, // Master table data
         Superord = x.eem.EMS_EXM_SUPERORD,
         ExaminerCategoryCode = x.eem.EMS_ECT_EXAMINER_CAT_CODE,
         Status = x.eem.RegisterStatus,
         SubSubId = x.eem.EMS_SUB_SUB_ID,
     })
     .ToListAsync();
            }
           

            return result;
        }

        public async Task<List<string>> GetSuperordsBySubSubIdAndPaperCodeAsync(string examCode,string subSubId, string paperCode,string activity,string regionCode)
        {
            var result = new List<string>();
            if (!string.IsNullOrEmpty(regionCode) && subSubId.StartsWith("7"))
            {



                var validSuperords = new[] { "BMS", "PMS", "RPMS", "DPMS" };

                result = await _context.EXAMINER_TRANSACTIONS
                .Where(a => a.EMS_SUB_SUB_ID == examCode + subSubId &&
                            a.EMS_PAPER_CODE == paperCode &&
                            a.EMS_ACTIVITY == activity &&
                            a.EMS_MARKING_REG_CODE == regionCode && a.RegisterStatus == "Present" &&
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
                            a.EMS_ACTIVITY == activity && a.RegisterStatus == "Present" &&
                            validSuperords.Contains(a.EMS_ECT_EXAMINER_CAT_CODE))  // Cleaner solution
                .Select(eem => eem.EMS_EXAMINER_NUMBER)
                .Distinct()
                .ToListAsync();


            }

            return result;
        }




        public async Task<Examiner> GetExaminer(string nationalId, string subKey, string examinerCode)
        {
            return await _context.EXM_EXAMINER_MASTER
                .FirstOrDefaultAsync(e =>
                    e.EMS_NATIONAL_ID == nationalId);
        }



        public async Task<OperationResult> RemoveExaminer(string nationalId,string userId)
        {

            var examiner = await _context.EXM_EXAMINER_MASTER
                .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == nationalId);

            if (examiner == null)
            {
                return new OperationResult { Success = false, Message = "Examiner not found" };
            }

            _context.EXM_EXAMINER_MASTER.Remove(examiner);
            await _context.SaveChangesAsync(userId);

            return new OperationResult { Success = true, Message = "Examiner removed successfully" };

        }


        public async Task<OperationResult> UpdateExaminerRecord(Examiner updatedExaminerData, string userId)
        {
            try
            {
                // Fetch the existing examiner record
                var examinerToUpdate = await _context.EXM_EXAMINER_MASTER
                    .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == updatedExaminerData.EMS_NATIONAL_ID);

                if (examinerToUpdate == null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Examiner not found"
                    };
                }

          
                // Update all modifiable properties using reflection
                foreach (var propertyInfo in typeof(Examiner).GetProperties())
                {
                    if (propertyInfo.CanWrite && propertyInfo.Name != nameof(examinerToUpdate.EMS_NATIONAL_ID))
                    {
                        var updatedValue = propertyInfo.GetValue(updatedExaminerData);
                        if (updatedValue != null)
                        {
                            propertyInfo.SetValue(examinerToUpdate, updatedValue);
                        }
                    }
                }

                // Mark entity as modified
                _context.Entry(examinerToUpdate).State = EntityState.Modified;

                // Save changes to the database with user tracking
                await _context.SaveChangesAsync(userId);

                return new OperationResult
                {
                    Success = true,
                    Message = "Record updated successfully"
                };
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                return new OperationResult
                {
                    Success = false,
                    Message = $"Record failed to update: {ex.Message}"
                };
            }
        }



        public async Task<OperationResult> AddOrReplaceExaminer(Examiner examiner,string userId)
        {
            try
            {
                // Check if the examiner with the same national ID, subkey, and examiner code already exists
                var existingExaminer = await _context.EXM_EXAMINER_MASTER
                    .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID &&
                                              e.EMS_SUBKEY == examiner.EMS_SUBKEY &&
                                              e.EMS_EXAMINER_CODE == examiner.EMS_EXAMINER_CODE);

                if (existingExaminer == null)
                {
                    // Generate a new examiner code if it already exists
                    if (await _context.EXM_EXAMINER_MASTER.AnyAsync(c => c.EMS_EXAMINER_CODE == examiner.EMS_EXAMINER_CODE))
                    {
                        var maxExaminerCodeStr = await _context.EXM_EXAMINER_MASTER.MaxAsync(u => u.EMS_EXAMINER_CODE);
                        if (double.TryParse(maxExaminerCodeStr, out double maxExaminerCode))
                        {
                            examiner.EMS_EXAMINER_CODE = (maxExaminerCode + 1).ToString();
                        }
                    }

                    // Check if the examiner number exists for the same subject and paper
                    var existingNumbers = await _context.EXM_EXAMINER_MASTER
                        .Where(c => c.EMS_EXAMINER_NUMBER == examiner.EMS_EXAMINER_NUMBER &&
                                    c.EMS_SUB_SUB_ID == examiner.EMS_SUB_SUB_ID &&
                                    c.EMS_PAPER_CODE == examiner.EMS_PAPER_CODE)
                        .ToListAsync();

                    if (!examiner.EMS_EXAMINER_CODE.StartsWith("000") && existingNumbers.Any())
                    {
                        var conflictingExaminers = existingNumbers
                            .Where(item => item.EMS_ECT_EXAMINER_CAT_CODE != "PBT" &&
                                           item.EMS_ECT_EXAMINER_CAT_CODE != "BT" &&
                                           item.EMS_ECT_EXAMINER_CAT_CODE != "A")
                            .Select(async item => await _context.ExaminerRegister
                                .FirstOrDefaultAsync(s => s.ExaminerCode == item.EMS_EXAMINER_CODE &&
                                                          s.EMS_SUBKEY == item.EMS_SUBKEY &&
                                                          s.IDNumber == item.EMS_NATIONAL_ID &&
                                                          s.Status == "Present"))
                            .Where(task => task.Result != null)
                            .Select(task => task.Result)
                            .ToList();

                        if (conflictingExaminers.Any())
                        {
                            return new OperationResult
                            {
                                Success = false,
                                Message = "Examiner Number Already Present"
                            };
                        }
                    }

                    // Assign default or validate examiner number based on category
                    if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "A")
                    {
                        examiner.EMS_EXAMINER_NUMBER ??= "0000";
                    }
                    else if (examiner.EMS_EXAMINER_NUMBER == null)
                    {
                        return new OperationResult
                        {
                            Success = false,
                            Message = "Examiner with This Examiner Number is not Available"
                        };
                    }

                    // Check if examiner with same national ID exists
                    if (await _context.EXM_EXAMINER_MASTER.AnyAsync(c => c.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID))
                    {
                        return new OperationResult
                        {
                            Success = false,
                            Message = "Examiner with the Same National ID Number already exists"
                        };
                    }

                    // Check for duplicate examiner name, last name, subject, and paper code
                    if (await _context.EXM_EXAMINER_MASTER.AnyAsync(c =>
                        c.EMS_EXAMINER_NAME == examiner.EMS_EXAMINER_NAME &&
                        c.EMS_LAST_NAME == examiner.EMS_LAST_NAME &&
                        c.EMS_SUB_SUB_ID == examiner.EMS_SUB_SUB_ID &&
                        c.EMS_PAPER_CODE == examiner.EMS_PAPER_CODE))
                    {
                        return new OperationResult
                        {
                            Success = false,
                            Message = "Examiner with the Same Details already exists"
                        };
                    }

                    //// Construct subkey if needed
                    //examiner.EMS_SUBKEY = examiner.EMS_SUB_SUB_ID != null && examiner.EMS_PAPER_CODE != null
                    //    ? examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + examiner.EMS_NATIONAL_ID
                    //    : examiner.EMS_SUBKEY;

                    // Set default examiner category if null
                    examiner.EMS_ECT_EXAMINER_CAT_CODE ??= "E";

                    // Prepare a new examiner object
                    var newExaminer = new Examiner
                    {
                        EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                        EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
                        EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                       EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD,
                        EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID,
                        EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME.ToUpper(),
                        EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                        EMS_SEX = examiner.EMS_SEX,
                        EMS_LAST_NAME = examiner.EMS_LAST_NAME.ToUpper(),
                        EMS_REGION_CODE = examiner.EMS_REGION_CODE,
                        EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE,
                        EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
                        EMS_LEVEL_OF_EXAM_MARKED = examiner.EMS_LEVEL_OF_EXAM_MARKED,
                        EMS_SUBKEY = examiner.EMS_SUBKEY
                    };

                    // Add the new examiner and save changes
                    _context.EXM_EXAMINER_MASTER.Add(newExaminer);
                    await _context.SaveChangesAsync(userId);

                    return new OperationResult
                    {
                        Success = true,
                        Message = "Examiner added successfully"
                    };
                }

                return new OperationResult
                {
                    Success = false,
                    Message = "Examiner with the provided details already exists"
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = $"An error occurred while adding the examiner. Please try again. {ex.Message}"
                };
            }
        }




        public async Task<OperationResult> AddNewExaminer(Examiner examiner,string userid)
        {
            try
            {
                if (examiner == null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Examiner details are required."
                    };
                }

                // Check if an examiner with the same National ID already exists
                var checkIdNumber = await _context.EXM_EXAMINER_MASTER
                    .AnyAsync(c => c.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID);

                if (checkIdNumber)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "An examiner with the same National ID already exists."
                    };
                }

                // Check if an examiner with the same details already exists
                var checkExaminerDetails = await _context.EXM_EXAMINER_MASTER
                    .AnyAsync(c => c.EMS_EXAMINER_NAME == examiner.EMS_EXAMINER_NAME &&
                                   c.EMS_LAST_NAME == examiner.EMS_LAST_NAME &&
                                   c.EMS_SUB_SUB_ID == examiner.EMS_SUB_SUB_ID &&
                                   c.EMS_PAPER_CODE == examiner.EMS_PAPER_CODE);

                if (checkExaminerDetails)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "An examiner with the same details already exists."
                    };
                }

                //// Assign default values for PBT or BT categories
                //if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT")
                //{
                //    examiner.EMS_SUB_SUB_ID = "9001";
                //    examiner.EMS_PAPER_CODE = "01";
                //    examiner.EMS_EXAMINER_NUMBER = "1001";
                //    examiner.EMS_EXM_SUPERORD = "1001";
                //}

             
                examiner.EMS_ECT_EXAMINER_CAT_CODE ??= "E";

                // Add the new examiner and save changes
                _context.EXM_EXAMINER_MASTER.Add(PrepareExaminerEntity(examiner));
                await _context.SaveChangesAsync(userid);

                return new OperationResult
                {
                    Success = true,
                    Message = "Examiner added successfully."
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = $"An error occurred while adding the examiner. Please try again. Error: {ex.Message}"
                };
            }
        }

        public async Task<bool> ImportExaminersFromOracle(string examLevel)
        {
            DataTable dataTable = new DataTable();

            using (OracleConnection oracle = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
            {
                oracle.Open();
                using (OracleCommand cmd = oracle.CreateCommand())
                {
                    cmd.CommandText = "SELECT EMS_NATIONAL_ID ,EMS_EXAMINER_CODE ,EMS_SUBKEY ,EMS_EXAMINER_NAME ,EMS_LAST_NAME ,EMS_SEX ,\r\n\t" +
                        "EMS_ADDRESS ,EMS_EXPERIENCE ,EMS_MARKING_EXPERIENCE ,EMS_LEVEL_OF_EXAM_MARKED ,EMS_STATUS ,\r\n\t" +
                        "EMS_COMMENTS ,EMS_PERFORMANCE_INDEX ,EMS_SELECTED_FLAG ,EMS_ECT_EXAMINER_CAT_CODE ,\r\n\t" +
                        "EMS_SUB_SUB_ID ,EMS_WORK_ADD1 ,EMS_WORK_ADD2 ,EMS_WORK_ADD3 ,EMS_ADDRESS2 ,EMS_ADDRESS3 ,\r\n\t" +
                        "EMS_WORK ,EMS_DISTRICT_CODE ,EMS_REGION_CODE ,EMS_PHONE_HOME ,EMS_PHONE_BUS ,\r\n\t" +
                        "EMS_QUALIFICATION ,EMS_PAPER_CODE ,EMS_MARKING_REG_CODE ,EMS_EXAMINER_NUMBER ,\r\n\t" +
                        "EMS_ACCOUNT_NO_FCA,EMS_BANK_NAME_FCA, NULL As EMS_BRANCH_NAME_FCA ,EMS_BANK_CODE_FCA ,EMS_BRANCH_CODE_FCA ,\r\n\t" +
                        "EMS_ACCOUNT_NO_ZWL, NULL As EMS_BRANCH_NAME_ZWL ,EMS_BANK_NAME_ZWL ,EMS_BANK_CODE_ZWL ,EMS_BRANCH_CODE_ZWL ,\r\n\t" +
                        "EMS_TAX_ID_NUMBER ,EMS_EXM_SUPERORD ,\r\n\t" +
                        "EMS_SEL_GRADING ,EMS_SEL_GRADE_REVIEW ,EMS_SEL_COORDINATION ,EMS_YEAR_TRAINED ,EMS_DATE_OF_JOINING, SYSDATE, 'admin' \r\n\t" +
                        "FROM ZIMSYS.EXM_EXAMINER_MASTER\r\n" +
                        "WHERE EMS_LEVEL_OF_EXAM_MARKED = :ExamLevel";
                    cmd.Parameters.Add("ExamLevel", examLevel);
                    OracleDataReader dataReader = await cmd.ExecuteReaderAsync();
                    dataTable.Load(dataReader);
                }
            }

            using (SqlConnection oracle = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                oracle.Open();

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_configuration.GetConnectionString("DefaultConnection")))
                {
                    bulkCopy.DestinationTableName = "[dbo].[EXM_EXAMINER_MASTER]";
                    bulkCopy.ColumnMappings.Add("EMS_NATIONAL_ID", "EMS_NATIONAL_ID");
                    bulkCopy.ColumnMappings.Add("EMS_EXAMINER_CODE", "EMS_EXAMINER_CODE");
                    bulkCopy.ColumnMappings.Add("EMS_SUBKEY", "EMS_SUBKEY");
                    bulkCopy.ColumnMappings.Add("EMS_EXAMINER_NAME", "EMS_EXAMINER_NAME");
                    bulkCopy.ColumnMappings.Add("EMS_LAST_NAME", "EMS_LAST_NAME");
                    bulkCopy.ColumnMappings.Add("EMS_SEX", "EMS_SEX");
                    bulkCopy.ColumnMappings.Add("EMS_ADDRESS", "EMS_ADDRESS");
                    bulkCopy.ColumnMappings.Add("EMS_EXPERIENCE", "EMS_EXPERIENCE");
                    bulkCopy.ColumnMappings.Add("EMS_MARKING_EXPERIENCE", "EMS_MARKING_EXPERIENCE");
                    bulkCopy.ColumnMappings.Add("EMS_LEVEL_OF_EXAM_MARKED", "EMS_LEVEL_OF_EXAM_MARKED");
                    bulkCopy.ColumnMappings.Add("EMS_STATUS", "EMS_STATUS");
                    bulkCopy.ColumnMappings.Add("EMS_COMMENTS", "EMS_COMMENTS");
                    bulkCopy.ColumnMappings.Add("EMS_PERFORMANCE_INDEX", "EMS_PERFORMANCE_INDEX");
                    bulkCopy.ColumnMappings.Add("EMS_SELECTED_FLAG", "EMS_SELECTED_FLAG");
                    bulkCopy.ColumnMappings.Add("EMS_ECT_EXAMINER_CAT_CODE", "EMS_ECT_EXAMINER_CAT_CODE");
                    bulkCopy.ColumnMappings.Add("EMS_SUB_SUB_ID", "EMS_SUB_SUB_ID");
                    bulkCopy.ColumnMappings.Add("EMS_WORK_ADD1", "EMS_WORK_ADD1");
                    bulkCopy.ColumnMappings.Add("EMS_WORK_ADD2", "EMS_WORK_ADD2");
                    bulkCopy.ColumnMappings.Add("EMS_WORK_ADD3", "EMS_WORK_ADD3");
                    bulkCopy.ColumnMappings.Add("EMS_ADDRESS2", "EMS_ADDRESS2");
                    bulkCopy.ColumnMappings.Add("EMS_ADDRESS3", "EMS_ADDRESS3");
                    bulkCopy.ColumnMappings.Add("EMS_WORK", "EMS_WORK");
                    bulkCopy.ColumnMappings.Add("EMS_DISTRICT_CODE", "EMS_DISTRICT_CODE");
                    bulkCopy.ColumnMappings.Add("EMS_REGION_CODE", "EMS_REGION_CODE");
                    bulkCopy.ColumnMappings.Add("EMS_PHONE_HOME", "EMS_PHONE_HOME");
                    bulkCopy.ColumnMappings.Add("EMS_PHONE_BUS", "EMS_PHONE_BUS");
                    bulkCopy.ColumnMappings.Add("EMS_QUALIFICATION", "EMS_QUALIFICATION");
                    bulkCopy.ColumnMappings.Add("EMS_PAPER_CODE", "EMS_PAPER_CODE");
                    bulkCopy.ColumnMappings.Add("EMS_MARKING_REG_CODE", "EMS_MARKING_REG_CODE");
                    bulkCopy.ColumnMappings.Add("EMS_EXAMINER_NUMBER", "EMS_EXAMINER_NUMBER");
                    bulkCopy.ColumnMappings.Add("EMS_ACCOUNT_NO_FCA", "EMS_ACCOUNT_NO_FCA");
                    bulkCopy.ColumnMappings.Add("EMS_BANK_NAME_FCA", "EMS_BANK_NAME_FCA");
                    bulkCopy.ColumnMappings.Add("EMS_BANK_CODE_FCA", "EMS_BANK_CODE_FCA");
                    bulkCopy.ColumnMappings.Add("EMS_BRANCH_CODE_FCA", "EMS_BRANCH_CODE_FCA");
                    bulkCopy.ColumnMappings.Add("EMS_ACCOUNT_NO_ZWL", "EMS_ACCOUNT_NO_ZWL");
                    bulkCopy.ColumnMappings.Add("EMS_BANK_NAME_ZWL", "EMS_BANK_NAME_ZWL");
                    bulkCopy.ColumnMappings.Add("EMS_BANK_CODE_ZWL", "EMS_BANK_CODE_ZWL");
                    bulkCopy.ColumnMappings.Add("EMS_BRANCH_CODE_ZWL", "EMS_BRANCH_CODE_ZWL");
                    bulkCopy.ColumnMappings.Add("EMS_TAX_ID_NUMBER", "EMS_TAX_ID_NUMBER");
                    bulkCopy.ColumnMappings.Add("EMS_EXM_SUPERORD", "EMS_EXM_SUPERORD");
                    bulkCopy.ColumnMappings.Add("EMS_SEL_GRADING", "EMS_SEL_GRADING");
                    bulkCopy.ColumnMappings.Add("EMS_SEL_GRADE_REVIEW", "EMS_SEL_GRADE_REVIEW");
                    bulkCopy.ColumnMappings.Add("EMS_SEL_COORDINATION", "EMS_SEL_COORDINATION");
                    bulkCopy.ColumnMappings.Add("EMS_YEAR_TRAINED", "EMS_YEAR_TRAINED");
                    bulkCopy.ColumnMappings.Add("EMS_DATE_OF_JOINING", "EMS_DATE_OF_JOINING");
                    try
                    {
                        bulkCopy.WriteToServer(dataTable);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<bool> ExportExaminersToOracle(string examLevel)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    try
                    {
                        cmd.CommandText = "SELECT EMS_NATIONAL_ID ,EMS_EXAMINER_CODE ,EMS_SUBKEY ,EMS_EXAMINER_NAME ,EMS_LAST_NAME ,EMS_SEX ,\r\n\t" +
                        "EMS_ADDRESS ,EMS_EXPERIENCE ,EMS_MARKING_EXPERIENCE ,EMS_LEVEL_OF_EXAM_MARKED ,EMS_STATUS ,\r\n\t" +
                        "EMS_COMMENTS ,EMS_PERFORMANCE_INDEX ,EMS_SELECTED_FLAG ,EMS_ECT_EXAMINER_CAT_CODE ,\r\n\t" +
                        "EMS_SUB_SUB_ID ,EMS_WORK_ADD1 ,EMS_WORK_ADD2 ,EMS_WORK_ADD3 ,EMS_ADDRESS2 ,EMS_ADDRESS3 ,\r\n\t" +
                        "EMS_WORK ,EMS_DISTRICT_CODE ,EMS_REGION_CODE ,EMS_PHONE_HOME ,EMS_PHONE_BUS ,\r\n\t" +
                        "EMS_QUALIFICATION ,EMS_PAPER_CODE ,EMS_MARKING_REG_CODE ,EMS_EXAMINER_NUMBER ,\r\n\t" +
                        "EMS_ACCOUNT_NO_FCA,EMS_BANK_NAME_FCA ,EMS_BANK_CODE_FCA ,EMS_BRANCH_CODE_FCA ,\r\n\t" +
                        "EMS_ACCOUNT_NO_ZWL,EMS_BANK_NAME_ZWL ,EMS_BANK_CODE_ZWL ,EMS_BRANCH_CODE_ZWL ,\r\n\t" +
                        "EMS_TAX_ID_NUMBER ,EMS_EXM_SUPERORD ,\r\n\t" +
                        "EMS_SEL_GRADING ,EMS_SEL_GRADE_REVIEW ,EMS_SEL_COORDINATION ,EMS_YEAR_TRAINED ,EMS_DATE_OF_JOINING \r\n\t" +
                        "FROM dbo.EXM_EXAMINER_MASTER\r\n" +
                        "WHERE EMS_LEVEL_OF_EXAM_MARKED = @ExamLevel";
                        cmd.Parameters.Add(new SqlParameter("ExamLevel", examLevel));
                        con.Open();
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        dataTable.Load(reader);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }

                }
            }


            DataTable newExaminers = dataTable.Clone();
            DataTable oldExaminers = dataTable.Clone();
            using (OracleConnection oracle = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
            {
                oracle.Open();

                
                    
                foreach (DataRow row in dataTable.Rows)
                {
                    DataTable dt = new DataTable();
                    using (OracleCommand command = oracle.CreateCommand())
                    {
                        command.CommandText = "Select * FROM ZIMSYS.EXM_EXAMINER_MASTER WHERE EMS_NATIONAL_ID = :NationalId";
                        command.Parameters.Add("NationalId", row["EMS_NATIONAL_ID"]);
                        OracleDataReader dataReader = await command.ExecuteReaderAsync();
                        dt.Load(dataReader);

                        if (dt.Rows.Count == 0)
                        {
                            // Create a new row in `newExaminers` DataTable and copy values
                            DataRow newRow = newExaminers.NewRow();
                            newRow.ItemArray = row.ItemArray; // Copy values from the original row
                            newExaminers.Rows.Add(newRow);
                        }
                        else
                        {
                            // Create a new row in `oldExaminers` DataTable and copy values
                            DataRow oldRow = oldExaminers.NewRow();
                            oldRow.ItemArray = row.ItemArray; // Copy values from the original row
                            oldExaminers.Rows.Add(oldRow);
                        }

                    }

                }
            }

            if (newExaminers.Rows.Count > 0)
            {
                using (OracleConnection oracle = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
                {
                    oracle.Open();

                    using (OracleBulkCopy bulkCopy = new OracleBulkCopy(_configuration.GetConnectionString("OracleConnection")))
                    {
                        bulkCopy.DestinationTableName = "ZIMSYS.EXM_EXAMINER_MASTER";
                        bulkCopy.ColumnMappings.Add("EMS_NATIONAL_ID", "EMS_NATIONAL_ID");
                        bulkCopy.ColumnMappings.Add("EMS_EXAMINER_CODE", "EMS_EXAMINER_CODE");
                        bulkCopy.ColumnMappings.Add("EMS_SUBKEY", "EMS_SUBKEY");
                        bulkCopy.ColumnMappings.Add("EMS_EXAMINER_NAME", "EMS_EXAMINER_NAME");
                        bulkCopy.ColumnMappings.Add("EMS_LAST_NAME", "EMS_LAST_NAME");
                        bulkCopy.ColumnMappings.Add("EMS_SEX", "EMS_SEX");
                        bulkCopy.ColumnMappings.Add("EMS_ADDRESS", "EMS_ADDRESS");
                        bulkCopy.ColumnMappings.Add("EMS_EXPERIENCE", "EMS_EXPERIENCE");
                        bulkCopy.ColumnMappings.Add("EMS_MARKING_EXPERIENCE", "EMS_MARKING_EXPERIENCE");
                        bulkCopy.ColumnMappings.Add("EMS_LEVEL_OF_EXAM_MARKED", "EMS_LEVEL_OF_EXAM_MARKED");
                        bulkCopy.ColumnMappings.Add("EMS_STATUS", "EMS_STATUS");
                        bulkCopy.ColumnMappings.Add("EMS_COMMENTS", "EMS_COMMENTS");
                        bulkCopy.ColumnMappings.Add("EMS_PERFORMANCE_INDEX", "EMS_PERFORMANCE_INDEX");
                        bulkCopy.ColumnMappings.Add("EMS_SELECTED_FLAG", "EMS_SELECTED_FLAG");
                        bulkCopy.ColumnMappings.Add("EMS_ECT_EXAMINER_CAT_CODE", "EMS_ECT_EXAMINER_CAT_CODE");
                        bulkCopy.ColumnMappings.Add("EMS_SUB_SUB_ID", "EMS_SUB_SUB_ID");
                        bulkCopy.ColumnMappings.Add("EMS_WORK_ADD1", "EMS_WORK_ADD1");
                        bulkCopy.ColumnMappings.Add("EMS_WORK_ADD2", "EMS_WORK_ADD2");
                        bulkCopy.ColumnMappings.Add("EMS_WORK_ADD3", "EMS_WORK_ADD3");
                        bulkCopy.ColumnMappings.Add("EMS_ADDRESS2", "EMS_ADDRESS2");
                        bulkCopy.ColumnMappings.Add("EMS_ADDRESS3", "EMS_ADDRESS3");
                        bulkCopy.ColumnMappings.Add("EMS_WORK", "EMS_WORK");
                        bulkCopy.ColumnMappings.Add("EMS_DISTRICT_CODE", "EMS_DISTRICT_CODE");
                        bulkCopy.ColumnMappings.Add("EMS_REGION_CODE", "EMS_REGION_CODE");
                        bulkCopy.ColumnMappings.Add("EMS_PHONE_HOME", "EMS_PHONE_HOME");
                        bulkCopy.ColumnMappings.Add("EMS_PHONE_BUS", "EMS_PHONE_BUS");
                        bulkCopy.ColumnMappings.Add("EMS_QUALIFICATION", "EMS_QUALIFICATION");
                        bulkCopy.ColumnMappings.Add("EMS_PAPER_CODE", "EMS_PAPER_CODE");
                        bulkCopy.ColumnMappings.Add("EMS_MARKING_REG_CODE", "EMS_MARKING_REG_CODE");
                        bulkCopy.ColumnMappings.Add("EMS_EXAMINER_NUMBER", "EMS_EXAMINER_NUMBER");
                        bulkCopy.ColumnMappings.Add("EMS_ACCOUNT_NO_FCA", "EMS_ACCOUNT_NO_FCA");
                        bulkCopy.ColumnMappings.Add("EMS_BANK_NAME_FCA", "EMS_BANK_NAME_FCA");
                        bulkCopy.ColumnMappings.Add("EMS_BANK_CODE_FCA", "EMS_BANK_CODE_FCA");
                        bulkCopy.ColumnMappings.Add("EMS_BRANCH_CODE_FCA", "EMS_BRANCH_CODE_FCA");
                        bulkCopy.ColumnMappings.Add("EMS_ACCOUNT_NO_ZWL", "EMS_ACCOUNT_NO_ZWL");
                        bulkCopy.ColumnMappings.Add("EMS_BANK_NAME_ZWL", "EMS_BANK_NAME_ZWL");
                        bulkCopy.ColumnMappings.Add("EMS_BANK_CODE_ZWL", "EMS_BANK_CODE_ZWL");
                        bulkCopy.ColumnMappings.Add("EMS_BRANCH_CODE_ZWL", "EMS_BRANCH_CODE_ZWL");
                        bulkCopy.ColumnMappings.Add("EMS_TAX_ID_NUMBER", "EMS_TAX_ID_NUMBER");
                        bulkCopy.ColumnMappings.Add("EMS_EXM_SUPERORD", "EMS_EXM_SUPERORD");
                        bulkCopy.ColumnMappings.Add("EMS_SEL_GRADING", "EMS_SEL_GRADING");
                        bulkCopy.ColumnMappings.Add("EMS_SEL_GRADE_REVIEW", "EMS_SEL_GRADE_REVIEW");
                        bulkCopy.ColumnMappings.Add("EMS_SEL_COORDINATION", "EMS_SEL_COORDINATION");
                        bulkCopy.ColumnMappings.Add("EMS_YEAR_TRAINED", "EMS_YEAR_TRAINED");
                        bulkCopy.ColumnMappings.Add("EMS_DATE_OF_JOINING", "EMS_DATE_OF_JOINING");
                        try
                        {
                            bulkCopy.WriteToServer(newExaminers);
                        }
                        catch (Exception e)
                        {
                            return false;
                        }
                    }
                }
            }
            if (oldExaminers.Rows.Count > 0)
            {
                foreach (DataRow row in oldExaminers.Rows)
                {
                    using (OracleConnection oracle = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
                    {
                        oracle.Open();

                        using (OracleCommand command = oracle.CreateCommand())
                        {
                            command.CommandText = "UPDATE ZIMSYS.EXM_EXAMINER_MASTER\r\n" +
                                "SET \r\n    " +
                                "EMS_EXAMINER_NAME = :EMS_EXAMINER_NAME,\r\n    " +
                                "EMS_LAST_NAME = :EMS_LAST_NAME,\r\n    " +
                                "EMS_SEX = :EMS_SEX,\r\n    " +
                                "EMS_ADDRESS = :EMS_ADDRESS,\r\n    " +
                                "EMS_EXPERIENCE = :EMS_EXPERIENCE,\r\n    " +
                                "EMS_MARKING_EXPERIENCE = :EMS_MARKING_EXPERIENCE,\r\n    " +
                                "EMS_STATUS = :EMS_STATUS,\r\n    " +
                                "EMS_COMMENTS = :EMS_COMMENTS,\r\n    " +
                                "EMS_PERFORMANCE_INDEX = :EMS_PERFORMANCE_INDEX,\r\n    " +
                                "EMS_ECT_EXAMINER_CAT_CODE = :EMS_ECT_EXAMINER_CAT_CODE,\r\n    " +
                                "EMS_SELECTED_FLAG = :EMS_SELECTED_FLAG,\r\n    " +
                                "EMS_SUB_SUB_ID = :EMS_SUB_SUB_ID,\r\n    " +
                                "EMS_WORK_ADD1 = :EMS_WORK_ADD1,\r\n    " +
                                "EMS_WORK_ADD2 = :EMS_WORK_ADD2,\r\n    " +
                                "EMS_WORK_ADD3 = :EMS_WORK_ADD3,\r\n    " +
                                "EMS_ADDRESS2 = :EMS_ADDRESS2,\r\n    " +
                                "EMS_ADDRESS3 = :EMS_ADDRESS3,\r\n    " +
                                "EMS_WORK = :EMS_WORK,\r\n    " +
                                "EMS_DISTRICT_CODE = :EMS_DISTRICT_CODE,\r\n    " +
                                "EMS_REGION_CODE = :EMS_REGION_CODE,\r\n    " +
                                "EMS_PHONE_HOME = :EMS_PHONE_HOME,\r\n    " +
                                "EMS_PHONE_BUS = :EMS_PHONE_BUS,\r\n    " +
                                "EMS_QUALIFICATION = :EMS_QUALIFICATION,\r\n    " +
                                "EMS_PAPER_CODE = :EMS_PAPER_CODE,\r\n    " +
                                "EMS_MARKING_REG_CODE = :EMS_MARKING_REG_CODE,\r\n    " +
                                "EMS_EXAMINER_NUMBER = :EMS_EXAMINER_NUMBER,\r\n    " +
                                "EMS_ACCOUNT_NO_FCA = :EMS_ACCOUNT_NO_FCA,\r\n    " +
                                "EMS_BANK_NAME_FCA = :EMS_BANK_NAME_FCA,\r\n    " +
                                "EMS_BANK_CODE_FCA = :EMS_BANK_CODE_FCA,\r\n    " +
                                "EMS_BRANCH_CODE_FCA = :EMS_BRANCH_CODE_FCA,\r\n    " +
                                "EMS_ACCOUNT_NO_ZWL = :EMS_ACCOUNT_NO_ZWL,\r\n    " +
                                "EMS_BANK_NAME_ZWL = :EMS_BANK_NAME_ZWL,\r\n    " +
                                "EMS_BANK_CODE_ZWL = :EMS_BANK_CODE_ZWL,\r\n    " +
                                "EMS_BRANCH_CODE_ZWL = :EMS_BRANCH_CODE_ZWL,\r\n    " +
                                "EMS_TAX_ID_NUMBER = :EMS_TAX_ID_NUMBER,\r\n    " +
                                "EMS_EXM_SUPERORD = :EMS_EXM_SUPERORD,\r\n    " +
                                "EMS_SEL_GRADING = :EMS_SEL_GRADING,\r\n    " +
                                "EMS_SEL_GRADE_REVIEW = :EMS_SEL_GRADE_REVIEW,\r\n    " +
                                "EMS_SEL_COORDINATION = :EMS_SEL_COORDINATION,\r\n    " +
                                "EMS_YEAR_TRAINED = :EMS_YEAR_TRAINED,\r\n    " +
                                "EMS_DATE_OF_JOINING = :EMS_DATE_OF_JOINING, \r\n" +
                                "EMS_SUBKEY = :EMS_SUBKEY\r\n    " +
                                "WHERE EMS_LEVEL_OF_EXAM_MARKED = :ExamLevel \r\n    " +
                                "AND EMS_NATIONAL_ID = :EMS_NATIONAL_ID";
                            command.Parameters.Add("EMS_EXAMINER_NAME", row["EMS_EXAMINER_NAME"]);
                            command.Parameters.Add("EMS_LAST_NAME", row["EMS_LAST_NAME"]);
                            command.Parameters.Add("EMS_SEX", row["EMS_SEX"]);
                            command.Parameters.Add("EMS_ADDRESS", row["EMS_ADDRESS"]);
                            command.Parameters.Add("EMS_EXPERIENCE", row["EMS_EXPERIENCE"]);
                            command.Parameters.Add("EMS_MARKING_EXPERIENCE", row["EMS_MARKING_EXPERIENCE"]);
                            command.Parameters.Add("EMS_STATUS", row["EMS_STATUS"]);
                            command.Parameters.Add("EMS_COMMENTS", row["EMS_COMMENTS"]);
                            command.Parameters.Add("EMS_PERFORMANCE_INDEX", row["EMS_PERFORMANCE_INDEX"]);
                            command.Parameters.Add("EMS_SELECTED_FLAG", row["EMS_SELECTED_FLAG"] ?? DBNull.Value );
                            command.Parameters.Add("EMS_ECT_EXAMINER_CAT_CODE", row["EMS_ECT_EXAMINER_CAT_CODE"]);
                            command.Parameters.Add("EMS_SUB_SUB_ID", row["EMS_SUB_SUB_ID"]);
                            command.Parameters.Add("EMS_WORK_ADD1", row["EMS_WORK_ADD1"]);
                            command.Parameters.Add("EMS_WORK_ADD2", row["EMS_WORK_ADD2"]);
                            command.Parameters.Add("EMS_WORK_ADD3", row["EMS_WORK_ADD3"]);
                            command.Parameters.Add("EMS_ADDRESS2", row["EMS_ADDRESS2"]);
                            command.Parameters.Add("EMS_ADDRESS3", row["EMS_ADDRESS3"]);
                            command.Parameters.Add("EMS_WORK", row["EMS_WORK"]);
                            command.Parameters.Add("EMS_DISTRICT_CODE", row["EMS_DISTRICT_CODE"]);
                            command.Parameters.Add("EMS_REGION_CODE", row["EMS_REGION_CODE"]);
                            command.Parameters.Add("EMS_PHONE_HOME", row["EMS_PHONE_HOME"]);
                            command.Parameters.Add("EMS_PHONE_BUS", row["EMS_PHONE_BUS"]);
                            command.Parameters.Add("EMS_QUALIFICATION", row["EMS_QUALIFICATION"]);
                            command.Parameters.Add("EMS_PAPER_CODE", row["EMS_PAPER_CODE"]);
                            command.Parameters.Add("EMS_MARKING_REG_CODE", row["EMS_MARKING_REG_CODE"]);
                            command.Parameters.Add("EMS_EXAMINER_NUMBER", row["EMS_EXAMINER_NUMBER"]);
                            command.Parameters.Add("EMS_ACCOUNT_NO_FCA", row["EMS_ACCOUNT_NO_FCA"]);
                            command.Parameters.Add("EMS_BANK_NAME_FCA", row["EMS_BANK_NAME_FCA"]);
                            command.Parameters.Add("EMS_BANK_CODE_FCA", row["EMS_BANK_CODE_FCA"]);
                            command.Parameters.Add("EMS_BRANCH_CODE_FCA", row["EMS_BRANCH_CODE_FCA"]);
                            command.Parameters.Add("EMS_ACCOUNT_NO_ZWL", row["EMS_ACCOUNT_NO_ZWL"]);
                            command.Parameters.Add("EMS_BANK_NAME_ZWL", row["EMS_BANK_NAME_ZWL"]);
                            command.Parameters.Add("EMS_BANK_CODE_ZWL", row["EMS_BANK_CODE_ZWL"]);
                            command.Parameters.Add("EMS_BRANCH_CODE_ZWL", row["EMS_BRANCH_CODE_ZWL"]);
                            command.Parameters.Add("EMS_TAX_ID_NUMBER", row["EMS_TAX_ID_NUMBER"]);
                            command.Parameters.Add("EMS_EXM_SUPERORD", row["EMS_EXM_SUPERORD"]);
                            command.Parameters.Add("EMS_SEL_GRADING", row["EMS_SEL_GRADING"]);
                            command.Parameters.Add("EMS_SEL_GRADE_REVIEW", row["EMS_SEL_GRADE_REVIEW"]);
                            command.Parameters.Add("EMS_SEL_COORDINATION", row["EMS_SEL_COORDINATION"]);
                            command.Parameters.Add("EMS_YEAR_TRAINED", row["EMS_YEAR_TRAINED"]);
                            command.Parameters.Add("EMS_DATE_OF_JOINING", row["EMS_DATE_OF_JOINING"]);
                            command.Parameters.Add("EMS_SUBKEY", row["EMS_SUBKEY"]);
                            command.Parameters.Add("EMS_LEVEL_OF_EXAM_MARKED", examLevel);
                            command.Parameters.Add("EMS_NATIONAL_ID", row["EMS_NATIONAL_ID"]);
                            command.Prepare();
                            try
                            {
                                var result = await command.ExecuteNonQueryAsync();
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                            
                        }
                    }
                }
            }

            return true;
        }

    
    

        private async Task<string> GenerateNewExaminerCodeIfExists(string existingExaminerCode)
        {
            var codeExists = await _context.EXM_EXAMINER_MASTER
                .AnyAsync(c => c.EMS_EXAMINER_CODE == existingExaminerCode);

            if (!codeExists)
                return existingExaminerCode;

            var maxExaminerCode = await _context.EXM_EXAMINER_MASTER.MaxAsync(e => e.EMS_EXAMINER_CODE);
            if (double.TryParse(maxExaminerCode, out double parsedCode))
            {
                return (parsedCode + 1).ToString("F0");
            }
            return existingExaminerCode; // Fallback if parsing fails
        }

        private Examiner PrepareExaminerEntity(Examiner examiner)
        {
            return new Examiner
            {
                EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
                EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD,
                EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE,
                EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID,
                EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME.ToUpper(),
                EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                EMS_SEX = examiner.EMS_SEX,
                EMS_LAST_NAME = examiner.EMS_LAST_NAME.ToUpper(),
                EMS_REGION_CODE = examiner.EMS_REGION_CODE,
                EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE,
                EMS_PAPER_CODE = examiner.EMS_PAPER_CODE,
                EMS_LEVEL_OF_EXAM_MARKED = examiner.EMS_LEVEL_OF_EXAM_MARKED,
                EMS_SUBKEY = examiner.EMS_SUBKEY,
                EMS_COMMENTS = examiner.EMS_COMMENTS
            };
        }

        public async Task<Examiner> GetLatestExaminerAsync()
        {
            return await _context.EXM_EXAMINER_MASTER
                .OrderByDescending(e => e.EMS_EXAMINER_CODE)
                .FirstOrDefaultAsync();
        }



        public async Task<IEnumerable<Examiner>> GetAllExaminers()
        {
            return await _context.EXM_EXAMINER_MASTER
          
                .ToListAsync();
        }


        public async Task<IEnumerable<Examiner>> GetComponentExaminers(string examCode,string subsbuid, string papercode,string activity,string regionCode)
        {
            var combinedSubSubId = examCode + subsbuid;

            var examiners = await _context.EXM_EXAMINER_MASTER
              .Where(et => et.EMS_SUB_SUB_ID == subsbuid && et.EMS_PAPER_CODE == papercode)
           
              .ToListAsync();

            if (!string.IsNullOrEmpty(regionCode))
            {
                examiners= examiners.Where(a => a.EMS_MARKING_REG_CODE == regionCode).ToList();
            }


            return examiners;

        }

     

        //public async Task<IEnumerable<Examiner>> GetPresentComponentExaminers(string examCode,string subsbuid, string papercode,string activity,string regionCode)
        //{
        //    var examiners = await _context.EXM_EXAMINER_MASTER.Where(a => a.EMS_SUB_SUB_ID == subsbuid && a.EMS_PAPER_CODE == papercode)   
        //        .ToListAsync();

        //    List<Examiner> examinerFinalList = new List<Examiner>();
        //    foreach (var examiner in examiners)
        //    {

        //        var checkTransaction = await _context.EXAMINER_TRANSACTIONS.FirstOrDefaultAsync(a => a.RegisterStatus == "Present" && a.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && a.EMS_ACTIVITY == activity && a.EMS_SUB_SUB_ID == (examCode + subsbuid) && a.EMS_PAPER_CODE == papercode );
        //        if (checkTransaction != null)
        //        {
        //            examinerFinalList.Add(examiner);
        //        }

        //    }

        //    if (!string.IsNullOrEmpty(regionCode))
        //    {
        //        examinerFinalList = examinerFinalList.Where(a => a.EMS_MARKING_REG_CODE == regionCode).ToList();
        //    }
        //    return examinerFinalList;
        //}


        public async Task<IEnumerable<Examiner>> GetExaminersByParameters(string subjectCode, string paperCode)
        {

           
            // Check if there are any records in EXAMINER_TRANSACTIONS table
            var examiners = await _context.EXM_EXAMINER_MASTER
                .Where(et => et.EMS_SUB_SUB_ID == subjectCode && et.EMS_PAPER_CODE == paperCode)
                .Include(a => a.RegisterDto)
                .ToListAsync();

            return examiners;

        }



        public async Task<OperationResult> ChangeRoleAndTeam(Examiner examiner,string userId)
        {
            try
            {
                var existingExaminer = await _context.EXM_EXAMINER_MASTER
       .FirstOrDefaultAsync(e =>  e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID);


                if (existingExaminer != null)
                {
                

                    // Update the properties of the existing examiner with the new values
                    existingExaminer.EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER;
                    existingExaminer.EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE;

                    existingExaminer.EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD;



                    _context.EXM_EXAMINER_MASTER.Update(existingExaminer);
                    await _context.SaveChangesAsync(userId);
                }
                else
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Examiner Not Found "
                    };
                }
                return new OperationResult
                {
                    Success = true,
                    Message = "Examiner record updated successfully"
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

        public async Task<OperationResult> AdminUpdateExaminerDetails(Examiner examiner,string examCode,string userId,string activity)
        {
            try
            {
                var existingExaminer = await _context.EXM_EXAMINER_MASTER
       .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID);


                if (existingExaminer != null)
                {
                   
                  
                        existingExaminer.EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID;
                    
            
                        existingExaminer.EMS_PAPER_CODE = examiner.EMS_PAPER_CODE;
             

                    if (examiner.EMS_MARKING_REG_CODE != null)
                    {
                        existingExaminer.EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE;

                    }

                    // Update the properties of the existing examiner with the new values
                    existingExaminer.EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER;
                    existingExaminer.EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE;
                    existingExaminer.EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD;
                    existingExaminer.EMS_REGION_CODE = examiner.EMS_REGION_CODE;
                    
                    _context.EXM_EXAMINER_MASTER.Update(existingExaminer);



                    await _context.SaveChangesAsync(userId);
                }
                else
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Examiner Not Found "
                    };
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


        public async Task<OperationResult> EditExaminer(Examiner examiner,string examCode,string userId,string activity,string attendance)
        {
            try
            {
                var existingExaminer = await _context.EXAMINER_TRANSACTIONS
       .FirstOrDefaultAsync(e =>  e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && e.EMS_SUBKEY == examiner.EMS_SUBKEY);


                if (existingExaminer != null)
                {
                 

                    if (examiner.EMS_MARKING_REG_CODE != null)
                    {
                        existingExaminer.EMS_MARKING_REG_CODE = examiner.EMS_MARKING_REG_CODE;

                    }
                    // Update the properties of the existing examiner with the new values
                    existingExaminer.EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER;
                    existingExaminer.EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE;
                    existingExaminer.EMS_EXM_SUPERORD = examiner.EMS_EXM_SUPERORD;
                    

                    if (activity != null)
                    {
                        existingExaminer.EMS_ACTIVITY = activity;

                    }

                    if (attendance != null)
                    {
                        existingExaminer.AttendanceStatus = attendance;

                    }

                    _context.EXAMINER_TRANSACTIONS.Update(existingExaminer);
                    await _context.SaveChangesAsync(userId);
                }
                else
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Examiner Not Found "
                    };
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

        public async Task<OperationResult> ChangeSubject(Examiner examiner,string userId)
        {
            try
            {
                var existingExaminer = await _context.EXM_EXAMINER_MASTER
           .FirstOrDefaultAsync(e =>  e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID);


                if (existingExaminer != null)
                {
                    var subKey = examiner.EMS_SUBKEY;
                    if (existingExaminer.EMS_SUB_SUB_ID != examiner.EMS_SUB_SUB_ID)
                    {
                        existingExaminer.EMS_SUB_SUB_ID = examiner.EMS_SUB_SUB_ID;
                        subKey = examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + examiner.EMS_NATIONAL_ID;
                        existingExaminer.EMS_SUBKEY = subKey;
                    }
                    if (existingExaminer.EMS_PAPER_CODE != examiner.EMS_PAPER_CODE)
                    {
                        subKey = examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + examiner.EMS_NATIONAL_ID;
                        existingExaminer.EMS_PAPER_CODE = examiner.EMS_PAPER_CODE;
                        existingExaminer.EMS_SUBKEY = subKey;
                    }

                    _context.EXM_EXAMINER_MASTER.Update(existingExaminer);



                    await _context.SaveChangesAsync(userId);
                }
                else
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Examiner Not Found "
                    };
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


        

        public async Task<Examiner> GetExaminerWithSUBKEY(string subKey, string examinerCode)
        {
            return await _context.EXM_EXAMINER_MASTER
                .FirstOrDefaultAsync(s => s.EMS_SUBKEY == subKey && s.EMS_EXAMINER_CODE == examinerCode);
        }


        public async Task<Examiner> GetExaminerRecord( string nationalId)
        {
            return await _context.EXM_EXAMINER_MASTER
                .Include(a => a.TandSs)
                .Include(a => a.ExaminerScriptsMarkeds)
                .FirstOrDefaultAsync(s => s.EMS_NATIONAL_ID == nationalId);
        }

        public async Task<List<ExaminerScriptsMarked>> GetPresentExaminersFromRegister(string examcode, string subjectCode, string paperCode)
        {
            var subSubId = examcode + subjectCode;

            // Fetch examiners and filter based on the register status
            var examiners = await _context.EXAMINER_TRANSACTIONS
                .Where(et => et.EMS_SUB_SUB_ID == subSubId && et.EMS_PAPER_CODE == paperCode && et.Examiner.RegisterDto.Status == "Present")
                .Include(a => a.Examiner)
                .Include(a => a.Examiner.RegisterDto)
                .ToListAsync();

            return examiners;

            //var presentExaminers = new List<Examiner>();

            //foreach (var examiner in examiners)
            //{
            //    var registerEntry = await _context.ExaminerRegister
            //        .FirstOrDefaultAsync(reg => reg.ExaminerCode == examiner.EMS_EXAMINER_CODE
            //            && reg.IDNumber == examiner.EMS_NATIONAL_ID
            //            && reg.EMS_SUBKEY == examiner.EMS_SUBKEY);

            //    if (registerEntry?.Status == "Present")
            //    {
            //        presentExaminers.Add(examiner);
            //    }
            //}

            //return presentExaminers;
        }



        public async Task<IEnumerable<Examiner>> GetPresentComponentExaminers(string examCode, string subsbuid, string papercode, string activity, string regionCode)
        {
            var combinedSubSubId = examCode + subsbuid;

      
            IQueryable<Examiner> query;



            if (!string.IsNullOrEmpty(regionCode))
            {
                query = from examiner in _context.EXM_EXAMINER_MASTER
                        join transaction in _context.EXAMINER_TRANSACTIONS
                        on examiner.EMS_NATIONAL_ID equals transaction.EMS_NATIONAL_ID
                        where 
                              transaction.RegisterStatus == "Present" &&
                              transaction.EMS_ACTIVITY == activity &&
                              transaction.EMS_SUB_SUB_ID == combinedSubSubId &&
                              transaction.EMS_PAPER_CODE == papercode &&
                               transaction.EMS_MARKING_REG_CODE == regionCode
                        select new Examiner
                        {
                            EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                            EMS_LAST_NAME = examiner.EMS_LAST_NAME,
                            EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
                            EMS_ECT_EXAMINER_CAT_CODE = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_SUBKEY = transaction.EMS_SUBKEY,
                            EMS_EXAMINER_NUMBER = transaction.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = transaction.EMS_EXM_SUPERORD,
                            EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID,
                         
                            EMS_MARKING_REG_CODE = transaction.EMS_MARKING_REG_CODE
                        };

         
            }
            else
            {
                query = from examiner in _context.EXM_EXAMINER_MASTER
                        join transaction in _context.EXAMINER_TRANSACTIONS
                        on examiner.EMS_NATIONAL_ID equals transaction.EMS_NATIONAL_ID
                        where 
                              transaction.RegisterStatus == "Present" &&
                              transaction.EMS_ACTIVITY == activity &&
                              transaction.EMS_SUB_SUB_ID == combinedSubSubId &&
                              transaction.EMS_PAPER_CODE == papercode 
                           
                        select new Examiner
                        {
                            EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                            EMS_LAST_NAME = examiner.EMS_LAST_NAME,
                            EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
                            EMS_ECT_EXAMINER_CAT_CODE = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_SUBKEY = transaction.EMS_SUBKEY,
                            EMS_EXAMINER_NUMBER = transaction.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = transaction.EMS_EXM_SUPERORD,
                            EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID,

                            EMS_MARKING_REG_CODE = transaction.EMS_MARKING_REG_CODE
                        };


            }

      
            var examinerFinalList = await query.ToListAsync();

            return examinerFinalList;
        }

        public async Task<IEnumerable<Examiner>> GetComponentExaminersTransaction(string examCode, string subsbuid, string papercode, string activity, string regionCode)
        {
            var combinedSubSubId = examCode + subsbuid;


            IQueryable<Examiner> query;



            if (!string.IsNullOrEmpty(regionCode))
            {
                query = from examiner in _context.EXM_EXAMINER_MASTER
                        join transaction in _context.EXAMINER_TRANSACTIONS
                        on examiner.EMS_NATIONAL_ID equals transaction.EMS_NATIONAL_ID
                        where 
                              transaction.EMS_ACTIVITY == activity &&
                              transaction.EMS_SUB_SUB_ID == combinedSubSubId &&
                              transaction.EMS_PAPER_CODE == papercode &&
                               transaction.EMS_MARKING_REG_CODE == regionCode
                        select new Examiner
                        {
                            EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                            EMS_LAST_NAME = examiner.EMS_LAST_NAME,
                            EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
                            EMS_ECT_EXAMINER_CAT_CODE = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_SUBKEY = transaction.EMS_SUBKEY,
                            EMS_EXAMINER_NUMBER = transaction.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = transaction.EMS_EXM_SUPERORD,
                            EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID,

                            EMS_MARKING_REG_CODE = transaction.EMS_MARKING_REG_CODE
                        };


            }
            else
            {
                query = from examiner in _context.EXM_EXAMINER_MASTER
                        join transaction in _context.EXAMINER_TRANSACTIONS
                        on examiner.EMS_NATIONAL_ID equals transaction.EMS_NATIONAL_ID
                        where 
                              transaction.EMS_ACTIVITY == activity &&
                              transaction.EMS_SUB_SUB_ID == combinedSubSubId &&
                              transaction.EMS_PAPER_CODE == papercode

                        select new Examiner
                        {
                            EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                            EMS_LAST_NAME = examiner.EMS_LAST_NAME,
                            EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
                            EMS_ECT_EXAMINER_CAT_CODE = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_SUBKEY = transaction.EMS_SUBKEY,
                            EMS_EXAMINER_NUMBER = transaction.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = transaction.EMS_EXM_SUPERORD,
                            EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID,

                            EMS_MARKING_REG_CODE = transaction.EMS_MARKING_REG_CODE
                        };


            }


            var examinerFinalList = await query.ToListAsync();

            return examinerFinalList;

        }






        public async Task<IEnumerable<Examiner>> GetAllComponentExaminersTransaction(string examCode, string activity)
        {

            IQueryable<Examiner> query;



                query = from examiner in _context.EXM_EXAMINER_MASTER
                        join transaction in _context.EXAMINER_TRANSACTIONS
                        on examiner.EMS_NATIONAL_ID equals transaction.EMS_NATIONAL_ID
                        where 
                              transaction.EMS_ACTIVITY == activity &&
                              
                              transaction.EMS_SUBKEY.StartsWith(examCode)

                        select new Examiner
                        {
                            EMS_NATIONAL_ID = examiner.EMS_NATIONAL_ID,
                            EMS_LAST_NAME = examiner.EMS_LAST_NAME,
                            EMS_EXAMINER_NAME = examiner.EMS_EXAMINER_NAME,
                            EMS_ECT_EXAMINER_CAT_CODE = transaction.EMS_ECT_EXAMINER_CAT_CODE,
                            EMS_SUBKEY = transaction.EMS_SUBKEY,
                            EMS_EXAMINER_NUMBER = transaction.EMS_EXAMINER_NUMBER,
                            EMS_EXM_SUPERORD = transaction.EMS_EXM_SUPERORD,
                            EMS_SUB_SUB_ID = transaction.EMS_SUB_SUB_ID,

                            EMS_MARKING_REG_CODE = transaction.EMS_MARKING_REG_CODE
                        };


            


            var examinerFinalList = await query.ToListAsync();

            return examinerFinalList;

        }


        public async Task<List<ApplicationUser>> GetUsersBySubkeys(IEnumerable<string> subkeys)
        {
            return await _context.Users
                .Where(u => subkeys.Contains(u.EMS_SUBKEY))
                .AsNoTracking()
                .ToListAsync();
        }



        public async Task<List<Examiner>> GetExaminersByIdNumbers(List<string> idNumbers)
        {
            return await _context.EXM_EXAMINER_MASTER
                .Where(e => idNumbers.Contains(e.EMS_NATIONAL_ID))
                .AsNoTracking() // This prevents entity tracking
                .ToListAsync();
        }





    }

    public class ExaminerDto
    {
        public string? ExaminerNumber { get; set; }
        public string? ExaminerName { get; set; }
        public string? Superord { get; set; }
        public string? ExaminerCategoryCode { get; set; }

        public string? Status { get;set; }

        public string? SubSubId {  get; set; }
    }

}
