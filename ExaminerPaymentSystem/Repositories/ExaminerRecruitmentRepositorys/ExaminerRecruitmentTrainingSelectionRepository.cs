using DocumentFormat.OpenXml.Drawing.Charts;
using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.ExaminerRecruitmentInterface;
using ExaminerPaymentSystem.Models.ExaminerRecruitment;
using ExaminerPaymentSystem.ViewModel.ExaminerRecutiments;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;

namespace ExaminerPaymentSystem.Repositories.ExaminerRecruitmentRepositorys
{
    // Repository Implementation
    public class ExaminerRecruitmentTrainingSelectionRepository : IExaminerRecruitmentTrainingSelectionRepository
    {
        private readonly ApplicationDbContext _context;

        public ExaminerRecruitmentTrainingSelectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

 

        public async Task<ExaminerRecruitmentTrainingSelection> GetByIdAsync(int id)
        {
            return await _context.ExaminerRecruitmentTrainingSelection.FindAsync(id);
        }

        public async Task<ExaminerRecruitmentTrainingSelection> GetByExaminerRecruitmentIdAsync(int id)
        {
            return await _context.ExaminerRecruitmentTrainingSelection
                                  .Where(e => e.ExaminerRecruitmentId == id)
                                  .FirstOrDefaultAsync();
        }

        public async Task AddAsync(ExaminerRecruitmentTrainingSelection entity)
        {
            await _context.ExaminerRecruitmentTrainingSelection.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExaminerRecruitmentTrainingSelection entity)
        {
            // If the entity is already tracked, detach it first
            var existingEntity = _context.ExaminerRecruitmentTrainingSelection.Local.FirstOrDefault(e => e.Id == entity.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = EntityState.Detached;
            }

            _context.ExaminerRecruitmentTrainingSelection.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.ExaminerRecruitmentTrainingSelection.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }


            public async Task<(List<ExaminerRecruitmentTrainingSelectionViewModel> Data, int TotalCount, int TotalExaminersWithRegisterTrue, int TotalExaminersWithRegisterFalse, int TotalSelectedNotRegistered, int TotalSelectedInTrainerTab)> GetPaginatedAsync(
             int skip,
             int take,
             string searchValue,
             string sortColumn,
             string sortDirection,
             string sessionLevelFilter,
             string subjectFilter,
             string paperCodeFilter,
             string regionCodeFilter)
        {
            var query = _context.ExaminerRecruitment
                .Include(er => er.ExaminerRecruitmentAttachements)
                .Include(er => er.TeachingExperiences)
                .Include(er => er.ExaminerRecruitmentTrainingSelection)
                .Include(er => er.ExaminerRecruitmentRegister)
                .Where(er => er.ExaminerRecruitmentTrainingSelection != null && er.ExaminerRecruitmentTrainingSelection.Status == true)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(er =>
                    er.LastName.ToLower().Contains(searchValue) ||
                    er.ExaminerName.ToLower().Contains(searchValue) ||
                    er.CemId.ToLower().Contains(searchValue) ||
                    er.EmailAddress.ToLower().Contains(searchValue) ||
                    er.PhoneHome.ToLower().Contains(searchValue) ||
                    er.Subject.ToLower().Contains(searchValue) ||
                    er.PaperCode.ToLower().Contains(searchValue) ||
                    er.Sex.ToLower().Contains(searchValue) ||
                    er.ExaminerCode.ToLower().Contains(searchValue)
                );
            }

            // Apply custom filters
            if (!string.IsNullOrWhiteSpace(sessionLevelFilter))
            {
                query = query.Where(e => e.Experience.ToLower().Contains(sessionLevelFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(subjectFilter))
            {
                query = query.Where(e => e.Subject.ToLower().Contains(subjectFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(paperCodeFilter))
            {
                query = query.Where(e => e.PaperCode.ToLower().Contains(paperCodeFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(regionCodeFilter))
            {
                query = query.Where(e => e.RegionCode.ToLower().Contains(regionCodeFilter.ToLower())); // Assuming CemId is the region code
            }

            // Calculate total count after applying filters
            var totalCount = await query.CountAsync();

            // Totals calculation
            var totalExaminersWithRegisterTrue = await _context.ExaminerRecruitmentRegisters
                .Where(er => er.Status == true)
                .CountAsync();

            var totalExaminersWithRegisterFalse = await _context.ExaminerRecruitmentRegisters
                .Where(er => er.Status == false)
                .CountAsync();

            // Calculate total for selected examiners not registered
            var totalSelectedInRegistered = await _context.ExaminerRecruitmentTrainingSelection
                .Where(er =>
                    er.ExaminerRecruitment.ExaminerRecruitmentRegister.ExaminerRecruitmentId != null) // Assuming null status means no registration
                .CountAsync();

            var totalSelectedInTrainerTab = await _context.ExaminerRecruitmentTrainingSelection
               .Where(er =>
                   er.ExaminerRecruitmentId != null) // Assuming null status means no registration
               .CountAsync();

            int totalSelectedNotRegistered = totalSelectedInTrainerTab - totalSelectedInRegistered;

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                var isAscending = sortDirection.ToLower() == "asc";
                query = isAscending
                    ? query.OrderBy(er => EF.Property<object>(er, sortColumn))
                    : query.OrderByDescending(er => EF.Property<object>(er, sortColumn));
            }
            else
            {
                query = query.OrderBy(er => er.Id); // Default sorting
            }

            // Apply pagination and select into the new ViewModel
            var data = await query
                .OrderByDescending(e => e.ExaminerRecruitmentTrainingSelection.Date)
                .Skip(skip)
                .Take(take)
                .Select(er => new ExaminerRecruitmentTrainingSelectionViewModel
                {
                    Status = er.ExaminerRecruitmentTrainingSelection.Status ? "Selected" : "Deselected",
                    Date = er.ExaminerRecruitmentTrainingSelection.Date,
                    ExaminerRecruitment = new List<ExaminerRecruimentDataTableViewModel>
                    {
                new ExaminerRecruimentDataTableViewModel
                {
                    Id = er.Id,
                    ExaminerCode = er.ExaminerCode,
                    ExaminerName = er.ExaminerName,
                    LastName = er.LastName,
                    PaperCode = er.PaperCode,
                    Subject = er.Subject,
                    CemId = er.CemId,
                    PhoneHome = er.PhoneHome,
                    EmailAddress = er.EmailAddress,
                    Gender = er.Sex,
                    Status = (bool)er.ExaminerRecruitmentRegister.Status,
                    AttachHeadComment = er.ExaminerRecruitmentAttachements.InstitutionHeadDoc,
                    AcademicQualifications = er.ExaminerRecruitmentAttachements.AcademicQualifications,
                    NationalIdDocs = er.ExaminerRecruitmentAttachements.NationalIdDocs

                    
                }
                    }
                })
                .ToListAsync();

            return (data, totalCount, totalExaminersWithRegisterTrue, totalExaminersWithRegisterFalse, totalSelectedNotRegistered, totalSelectedInTrainerTab);
        }





        public async Task<(List<ExaminerRecruitmentAndSelectionViewModel> Data, int TotalCount, int TotalExaminersWithRegisterTrue, int TotalExaminersWithRegisterFalse, int TotalSelectedNotRegistered, int TotalSelectedInTrainerTab)> GetPaginatedRegisterAsync(
            int skip,
            int take,
            string searchValue,
            string sortColumn,
            string sortDirection,
            string subjectFilter,
            string paperCodeFilter,
            string experienceFilter)
        {
            var query = _context.ExaminerRecruitment
                .Include(er => er.ExaminerRecruitmentTrainingSelection)
                .Include(er => er.ExaminerRecruitmentRegister)
                .Where(er => er.ExaminerRecruitmentTrainingSelection != null && er.ExaminerRecruitmentTrainingSelection.Status == true)
                .AsQueryable();

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(er =>
                    er.LastName.ToLower().Contains(searchValue) ||
                    er.ExaminerName.ToLower().Contains(searchValue) ||
                    er.CemId.ToLower().Contains(searchValue) ||
                    er.EmailAddress.ToLower().Contains(searchValue) ||
                    er.PhoneHome.ToLower().Contains(searchValue) ||
                    er.Subject.ToLower().Contains(searchValue) ||
                    er.PaperCode.ToLower().Contains(searchValue) ||
                    er.Sex.ToLower().Contains(searchValue));
            }

            // Apply custom filters
            if (!string.IsNullOrWhiteSpace(subjectFilter))
                query = query.Where(e => e.Subject.ToLower().Contains(subjectFilter.ToLower()));

            if (!string.IsNullOrWhiteSpace(paperCodeFilter))
                query = query.Where(e => e.PaperCode.ToLower().Contains(paperCodeFilter.ToLower()));

            if (!string.IsNullOrWhiteSpace(experienceFilter))
                query = query.Where(e => e.Experience.ToLower().Contains(experienceFilter.ToLower()));

            // Calculate total count after applying filters
            var totalCount = await query.CountAsync();

            // Calculate filtered totals
            var filteredRegisterQuery = query.Where(er => er.ExaminerRecruitmentRegister != null);

            var totalExaminersWithRegisterTrue = await filteredRegisterQuery.CountAsync(er => er.ExaminerRecruitmentRegister.Status == true);
            var totalExaminersWithRegisterFalse = await filteredRegisterQuery.CountAsync(er => er.ExaminerRecruitmentRegister.Status == false);

            var totalSelectedInRegistered = await query.CountAsync(er =>
                er.ExaminerRecruitmentRegister != null && er.ExaminerRecruitmentRegister.ExaminerRecruitmentId != null);

            var totalSelectedInTrainerTab = await query.CountAsync(er => er.Id != null);

            int totalSelectedNotRegistered = totalSelectedInTrainerTab - totalSelectedInRegistered;

            // Apply sorting
            query = sortColumn switch
            {
                "Subject" => sortDirection == "asc" ? query.OrderBy(e => e.Subject) : query.OrderByDescending(e => e.Subject),
                "Experience" => sortDirection == "asc" ? query.OrderBy(e => e.Experience) : query.OrderByDescending(e => e.Experience),
                "PaperCode" => sortDirection == "asc" ? query.OrderBy(e => e.PaperCode) : query.OrderByDescending(e => e.PaperCode),
                _ => query.OrderByDescending(e => e.Id) // Default sorting
            };

            // Apply pagination and select into the ViewModel
            var data = await query
                .OrderByDescending(e => e.ExaminerRecruitmentTrainingSelection.Date)
                .Skip(skip)
                .Take(take)
                .Select(er => new ExaminerRecruitmentAndSelectionViewModel
                {
               
                    ExaminerRecruitmentId = er.Id,
                    ExaminerName = er.ExaminerName,
                    LastName = er.LastName,
                    PaperCode = er.PaperCode,
                    Subject = er.Subject,
                    CemId = er.CemId,
                    Experience = er.Experience,
                    PhoneHome = er.PhoneHome,
                    EmailAddress = er.EmailAddress,
                    Gender = er.Sex,
                    RegisterStatus = er.ExaminerRecruitmentRegister.Status,

                   
                   
                })
                .ToListAsync();

            return (data, totalCount, totalExaminersWithRegisterTrue, totalExaminersWithRegisterFalse, totalSelectedNotRegistered, totalSelectedInTrainerTab);
        }




        public async Task<(List<ExaminerRecruitmentTrainingSelectionViewModel> Data, int TotalCount)> GetDeselectedExaminersAsync(
            int skip,
            int take,
            string searchValue,
            string sortColumn,
            string sortDirection,
            string sessionLevelFilter,
             string subjectFilter,
             string paperCodeFilter,
             string regionCodeFilter)
        {
            var query = _context.ExaminerRecruitment
                .Include(er => er.ExaminerRecruitmentAttachements)
                .Include(er => er.TeachingExperiences)
                .Include(er => er.ExaminerRecruitmentTrainingSelection)
                .Where(er => er.ExaminerRecruitmentTrainingSelection != null && er.ExaminerRecruitmentTrainingSelection.Status == false)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(er =>
                    er.LastName.ToLower().Contains(searchValue) ||
                    er.ExaminerName.ToLower().Contains(searchValue) ||
                    er.CemId.ToLower().Contains(searchValue) ||
                    er.EmailAddress.ToLower().Contains(searchValue) ||
                    er.PhoneHome.ToLower().Contains(searchValue) ||
                    er.Subject.ToLower().Contains(searchValue) ||
                    er.PaperCode.ToLower().Contains(searchValue) ||
                     er.Sex.ToLower().Contains(searchValue) ||
                    er.ExaminerCode.ToLower().Contains(searchValue)
                );
            }

            // Apply custom filters

            if (!string.IsNullOrWhiteSpace(sessionLevelFilter))
            {
                query = query.Where(e => e.Experience.ToLower().Contains(sessionLevelFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(subjectFilter))
            {
                query = query.Where(e => e.Subject.ToLower().Contains(subjectFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(paperCodeFilter))
            {
                query = query.Where(e => e.PaperCode.ToLower().Contains(paperCodeFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(regionCodeFilter))
            {
                query = query.Where(e => e.RegionCode.ToLower().Contains(regionCodeFilter.ToLower())); // Assuming CemId is the region code
            }


            // Calculate total count after applying filters
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                var isAscending = sortDirection.ToLower() == "asc";
                query = isAscending
                    ? query.OrderBy(er => EF.Property<object>(er, sortColumn))
                    : query.OrderByDescending(er => EF.Property<object>(er, sortColumn));
            }
            else
            {
                query = query.OrderBy(er => er.Id); // Default sorting
            }

            // Apply pagination and select into the new ViewModel
            var data = await query
                .OrderByDescending(e => e.ExaminerRecruitmentTrainingSelection.Date)
                .Skip(skip)
                .Take(take)
                .Select(er => new ExaminerRecruitmentTrainingSelectionViewModel
                {
                    Status = er.ExaminerRecruitmentTrainingSelection.Status ? "Selected" : "Deselected",
                    Date = er.ExaminerRecruitmentTrainingSelection.Date,
                    ExaminerRecruitment = new List<ExaminerRecruimentDataTableViewModel>
                    {
                new ExaminerRecruimentDataTableViewModel
                {
                    Id = er.Id,
                    ExaminerCode =er.ExaminerCode,
                    ExaminerName = er.ExaminerName,
                    LastName = er.LastName,
                    PaperCode = er.PaperCode,
                    Subject = er.Subject,
                    CemId = er.CemId,
                    PhoneHome = er.PhoneHome,
                    EmailAddress = er.EmailAddress,
                    Gender = er.Sex,
                }
                    }
                })
                .ToListAsync();

            return (data, totalCount);
        }



        public async Task<(IEnumerable<ExaminerRecruitmentTrainingSelectionViewModel>, int, int)> GetTrainingSelectionDataTableAsync(
        int start, int length, string searchValue, string sortColumn, string sortDirection,
        string subject = null, string experience = null, string paperCode = null, string region = null)
        {
            var query = _context.ExaminerRecruitmentTrainingSelection
                .Include(e => e.ExaminerRecruitment)
                .Where(e => e.Status == true)
                .AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(subject))
                query = query.Where(e => e.ExaminerRecruitment.Subject == subject);

            if (!string.IsNullOrEmpty(experience))
                query = query.Where(e => e.ExaminerRecruitment.Experience == experience);

            if (!string.IsNullOrEmpty(paperCode))
                query = query.Where(e => e.ExaminerRecruitment.PaperCode == paperCode);

            if (!string.IsNullOrEmpty(region))
                query = query.Where(e => e.ExaminerRecruitment.RegionCode == region);

            // Count total before filtering
            var totalRecords = await _context.ExaminerRecruitmentTrainingSelection.CountAsync();

            // Apply search
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e => e.ExaminerRecruitment.Subject.Contains(searchValue) ||
                                         e.ExaminerRecruitment.Experience.Contains(searchValue) ||
                                         e.ExaminerRecruitment.PaperCode.Contains(searchValue) ||
                                         e.ExaminerRecruitment.CemId.Contains(searchValue) ||
                                         e.ExaminerRecruitment.EmailAddress.Contains(searchValue) ||
                                         e.ExaminerRecruitment.ExaminerName.Contains(searchValue) ||
                                         e.ExaminerRecruitment.LastName.Contains(searchValue) ||
                                         e.ExaminerRecruitment.PhoneHome.Contains(searchValue) ||
                                         e.ExaminerRecruitment.Sex.Contains(searchValue) ||
                                         e.ExaminerRecruitment.RegionCode.Contains(searchValue));
            }

            // Count filtered records
            var filteredRecords = await query.CountAsync();

            // Apply sorting
            query = sortColumn switch
            {
                "Subject" => sortDirection == "asc" ? query.OrderBy(e => e.ExaminerRecruitment.Subject) : query.OrderByDescending(e => e.ExaminerRecruitment.Subject),
                "Experience" => sortDirection == "asc" ? query.OrderBy(e => e.ExaminerRecruitment.Experience) : query.OrderByDescending(e => e.ExaminerRecruitment.Experience),
                "PaperCode" => sortDirection == "asc" ? query.OrderBy(e => e.ExaminerRecruitment.PaperCode) : query.OrderByDescending(e => e.ExaminerRecruitment.PaperCode),
                "Region" => sortDirection == "asc" ? query.OrderBy(e => e.ExaminerRecruitment.RegionCode) : query.OrderByDescending(e => e.ExaminerRecruitment.RegionCode),
                _ => query.OrderByDescending(e => e.ExaminerRecruitment.CaptureDate)
            };

            // Apply pagination and project to ViewModel
            var data = await query
                .Skip(start)
                .Take(length)
                .Select(e => new ExaminerRecruitmentTrainingSelectionViewModel
                {
                    ExaminerName = e.ExaminerRecruitment.ExaminerName,
                    LastName = e.ExaminerRecruitment.LastName,
                    CemId = e.ExaminerRecruitment.CemId,
                    EmailAddress = e.ExaminerRecruitment.EmailAddress,
                    Gender = e.ExaminerRecruitment.Sex,
                    PhoneHome = e.ExaminerRecruitment.PhoneHome,
                    Subject = e.ExaminerRecruitment.Subject,
                    Experience = e.ExaminerRecruitment.Experience,
                    PaperCode = e.ExaminerRecruitment.PaperCode,
                    RegionCode = e.ExaminerRecruitment.RegionCode,
                    ExaminerRecruitmentId = e.ExaminerRecruitment.Id
                })
                .ToListAsync();

            return (data, totalRecords, filteredRecords);
        }




        public async Task<int[]> GetIdsForBulkDownload(
             string Subject = null,
             string Experience = null,
             string PaperCode = null,
             string Region = null)
        {
            var query = _context.ExaminerRecruitmentTrainingSelection
                .Include(e => e.ExaminerRecruitment)
                .Where(e => e.Status == true)
                .AsNoTracking();

            // Apply filters only if values are provided
            if (!string.IsNullOrEmpty(Subject))
                query = query.Where(e => e.ExaminerRecruitment.Subject == Subject);

            if (!string.IsNullOrEmpty(Experience))
                query = query.Where(e => e.ExaminerRecruitment.Experience == Experience);

            if (!string.IsNullOrEmpty(PaperCode))
                query = query.Where(e => e.ExaminerRecruitment.PaperCode == PaperCode);

            if (!string.IsNullOrEmpty(Region))
                query = query.Where(e => e.ExaminerRecruitment.RegionCode == Region);

            // Return only the matching IDs
            return await query.Select(e => e.ExaminerRecruitment.Id).ToArrayAsync();
        }


        public Task<IEnumerable<ExaminerRecruitmentTrainingSelection>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

    }

}
