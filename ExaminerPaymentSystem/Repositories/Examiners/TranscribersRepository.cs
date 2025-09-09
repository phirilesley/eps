using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Transcribers;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using ExaminerPaymentSystem.ViewModels.Examiners;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class TranscribersRepository: ITranscribersRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly UserManager<ApplicationUser> _userManager;

        public TranscribersRepository(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }



        public async Task<OperationResult> AddNewTranscriber(Examiner examiner,string userid)
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

                // Check for duplicate national ID
                var existingExaminerById = await _context.EXM_EXAMINER_MASTER
                    .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID);
                if (existingExaminerById != null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "An examiner with the same National ID already exists."
                    };
                }

                // Check for duplicate examiner details
                var existingExaminerByDetails = await _context.EXM_EXAMINER_MASTER
                    .FirstOrDefaultAsync(e => e.EMS_EXAMINER_NAME == examiner.EMS_EXAMINER_NAME &&
                                              e.EMS_LAST_NAME == examiner.EMS_LAST_NAME &&
                                              e.EMS_SUB_SUB_ID == examiner.EMS_SUB_SUB_ID &&
                                              e.EMS_PAPER_CODE == examiner.EMS_PAPER_CODE);
                if (existingExaminerByDetails != null)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "An examiner with the same details already exists."
                    };
                }

                // Handle specific category settings
                if (examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "S" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "I" || examiner.EMS_ECT_EXAMINER_CAT_CODE == "A")
                {
                    examiner.EMS_SUB_SUB_ID = "9001";
                    examiner.EMS_PAPER_CODE = "01";
                    examiner.EMS_EXAMINER_NUMBER = "9001";
                }

                //// Generate a new examiner code if necessary
                //if (await _context.EXM_EXAMINER_MASTER.AnyAsync(e => e.EMS_EXAMINER_CODE == examiner.EMS_EXAMINER_CODE))
                //{
                //    var maxCode = await _context.EXM_EXAMINER_MASTER.MaxAsync(e => e.EMS_EXAMINER_CODE);
                //    if (double.TryParse(maxCode, out double parsedCode))
                //    {
                //        examiner.EMS_EXAMINER_CODE = (parsedCode + 1).ToString("F0");
                //    }
                //}

                // Assign subkey if applicable
                //examiner.EMS_SUBKEY = string.IsNullOrEmpty(examiner.EMS_SUB_SUB_ID) || string.IsNullOrEmpty(examiner.EMS_PAPER_CODE)
                //    ? examiner.EMS_SUBKEY
                //    : examiner.EMS_SUB_SUB_ID + examiner.EMS_PAPER_CODE + examiner.EMS_NATIONAL_ID;

                // Default category code
                examiner.EMS_ECT_EXAMINER_CAT_CODE ??= "BT";

                // Prepare and add the new examiner entity
                var newExaminer = new Examiner
                {
                    EMS_EXAMINER_CODE = examiner.EMS_EXAMINER_CODE,
                    EMS_EXAMINER_NUMBER = examiner.EMS_EXAMINER_NUMBER,
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

                _context.EXM_EXAMINER_MASTER.Add(newExaminer);
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


        public async Task<OperationResult> EditTranscribers(Examiner examiner,string attendance,string activity,string userId)
        {
            try
            {
                var existingExaminer = await _context.EXAMINER_TRANSACTIONS
       .FirstOrDefaultAsync(e => e.EMS_NATIONAL_ID == examiner.EMS_NATIONAL_ID && e.EMS_ACTIVITY == activity);


                if (existingExaminer != null)
                {
            
                    existingExaminer.EMS_ECT_EXAMINER_CAT_CODE = examiner.EMS_ECT_EXAMINER_CAT_CODE;

                    if(activity != null)
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

    

        public async Task<List<ExaminersListModel>> GetTranscribersFromMaster()
        {
         
            var transcriberExaminers = await _context.EXM_EXAMINER_MASTER
                .Where(et => et.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || et.EMS_ECT_EXAMINER_CAT_CODE == "BT" || et.EMS_ECT_EXAMINER_CAT_CODE == "A" || et.EMS_ECT_EXAMINER_CAT_CODE == "S" || et.EMS_ECT_EXAMINER_CAT_CODE == "I"
                             )
                
                .Select(a =>  new ExaminersListModel {

                    ExaminerCode = a.EMS_EXAMINER_CODE,
                    FirstName = a.EMS_EXAMINER_NAME,
                    LastName = a.EMS_LAST_NAME,
                    IDNumber = a.EMS_NATIONAL_ID,
                    SubKey = a.EMS_SUBKEY,
                    BMS = a.EMS_EXM_SUPERORD,
                    ExaminerNumber = a.EMS_EXAMINER_NUMBER,
                    Category = a.EMS_ECT_EXAMINER_CAT_CODE,

                })

                .ToListAsync();

            return transcriberExaminers;
 
        }

        public async Task<List<ExaminersListModel>> GetSelectedTranscribersFromTransaction(string examCode,string activity)
        {

            var transcriberExaminers = await _context.EXAMINER_TRANSACTIONS
                .Where(et => (et.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || et.EMS_ECT_EXAMINER_CAT_CODE == "BT" || et.EMS_ECT_EXAMINER_CAT_CODE == "A" || et.EMS_ECT_EXAMINER_CAT_CODE == "S" || et.EMS_ECT_EXAMINER_CAT_CODE == "I") && et.EMS_SUBKEY.StartsWith(examCode)
                && et.EMS_ACTIVITY == activity
                             )
                .Include(a => a.Examiner)

                .Select(a => new ExaminersListModel
                {

                    ExaminerCode = a.EMS_EXAMINER_CODE,
                    FirstName = a.Examiner.EMS_EXAMINER_NAME,
                    LastName = a.Examiner.EMS_LAST_NAME,
                    IDNumber = a.EMS_NATIONAL_ID,
                    SubKey = a.EMS_SUBKEY,
                    BMS = a.EMS_EXM_SUPERORD,
                    ExaminerNumber = a.EMS_EXAMINER_NUMBER,
                    Category = a.EMS_ECT_EXAMINER_CAT_CODE,
                    Status = a.RegisterStatus

                })

                .ToListAsync();

            return transcriberExaminers;

        }



        public async Task<IEnumerable<SelectTeamViewModel>> GetTeamsFromMasterAsync()
        {

            try
            {
                List<SelectTeamViewModel> teamview = new List<SelectTeamViewModel>();
                var allComponentExaminers = await _context.EXM_EXAMINER_MASTER
      .Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "A" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I")
      .Include(a => a.ExaminerScriptsMarkeds)
      .ToListAsync();

            
                foreach (var component in allComponentExaminers)
                {
                    string station = string.IsNullOrEmpty(component.EMS_WORK_ADD1)
    ? string.Empty
    : component.EMS_WORK_ADD1.Substring(0, Math.Min(15, component.EMS_WORK_ADD1.Length));
                  
                    var transaction = component.ExaminerScriptsMarkeds.FirstOrDefault(a => a.EMS_NATIONAL_ID == component.EMS_NATIONAL_ID );

                    if (transaction != null)
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
                            ExaminerNumber = component.EMS_EXAMINER_NUMBER,
                            Team = component.EMS_EXM_SUPERORD,
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

        public async Task<IEnumerable<TandS>> GetAllTandSForTranscribers(string venue,string examCode,string activity)
        {
            try
            {
                var a = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                    .Where(s => s.EMS_VENUE == venue && s.EMS_SUBKEY.StartsWith(examCode) && s.EMS_PURPOSEOFJOURNEY == activity &&
                                (s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT" || s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT" ||s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "A" || s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "S" || s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "I")
                                && _context.EXAMINER_TRANSACTIONS
                                   .Any(a => a.EMS_SUBKEY == s.EMS_SUBKEY && a.RegisterStatus == "Present")
                                )
                    .Include(a => a.Examiner)
                     .Include(a => a.Examiner.ExaminerScriptsMarkeds)
                      .Include(t => t.TandSDetails)
                                 .Include(t => t.TandSAdvance)
                                 .Include(t => t.TandSFiles)

                    .ToListAsync();

                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TandS>> GetFullListTandSForTranscribers()
        {
            try
            {
                var a = await _context.TRAVELLING_AND_SUBSISTENCE_CLAIM
                    .Where(s => 
                                s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "BT" || s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "PBT" ||s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "A" || s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "S" || s.Examiner.EMS_ECT_EXAMINER_CAT_CODE == "I")
                    .Include(a => a.Examiner)
                      .Include(t => t.TandSDetails)
                                 .Include(t => t.TandSAdvance)
                                 .Include(t => t.TandSFiles)

                    .ToListAsync();

                return a;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<IEnumerable<ExaminerScriptsMarked>> GetTranscribersRegister()
        {
            var trascribers = await _context.EXAMINER_TRANSACTIONS.Where(a => a.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || a.EMS_ECT_EXAMINER_CAT_CODE == "BT" || a.EMS_ECT_EXAMINER_CAT_CODE == "A" || a.EMS_ECT_EXAMINER_CAT_CODE == "S" || a.EMS_ECT_EXAMINER_CAT_CODE == "I")
                .Include(t => t.Examiner)
                .ToListAsync();

            return trascribers;
        }

        public async Task<IEnumerable<Examiner>> GetAllTranscribers()
        {

            // Check if there are any records in EXAMINER_TRANSACTIONS table
            var examiners = await _context.EXM_EXAMINER_MASTER
                .Where(et => et.EMS_ECT_EXAMINER_CAT_CODE == "BT" || et.EMS_ECT_EXAMINER_CAT_CODE == "PBT" || et.EMS_ECT_EXAMINER_CAT_CODE == "A" || et.EMS_ECT_EXAMINER_CAT_CODE == "S" || et.EMS_ECT_EXAMINER_CAT_CODE == "I")
                .ToListAsync();


            return examiners;
        }


    }
}
