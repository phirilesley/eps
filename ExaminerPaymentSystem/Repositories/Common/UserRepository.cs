using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Major;
using ExaminerPaymentSystem.Models.Common;
using ExaminerPaymentSystem.Models.Major;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace ExaminerPaymentSystem.Repositories.Common
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _context;
        private readonly ApplicationDbContext _datacontext;
        public UserRepository(UserManager<ApplicationUser> context, ApplicationDbContext datacontext)
        {
            _context = context;
            _datacontext = datacontext;
        }
        public async Task<IEnumerable<ApplicationUser>> UsersGetAll()
        {
            try
            {
                return await _context.
                    Users
                   .Include(a => a.Examiner)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> UsersGetAllZiMSEC()
        {
            try
            {
                var excludedUsernames = new List<string> { "subjectmanager", "centresupervisor", "accounts", "peerreviewer" };

                var users = await _context.Users
                    .Where(u => u.ExaminerCode.StartsWith("00") && !excludedUsernames.Contains(u.UserName))
                    .ToListAsync();

                return users;
            }
            catch (Exception)
            {
                throw;
            }
        }


        //    public async Task<IEnumerable<ApplicationUser>> UsersGetAllComponent(
        //string examCode, string subjectCode, string paperCode, string regionCode, string activity)
        //    {
        //        try
        //        {
        //            var key = examCode + subjectCode;

        //            var usersList = await (from user in _datacontext.Users
        //                                   join transaction in _datacontext.EXAMINER_TRANSACTIONS
        //                                   on new { user.EMS_SUBKEY, user.IDNumber }
        //                                   equals new { transaction.EMS_SUBKEY, EMS_NATIONAL_ID = transaction.EMS_NATIONAL_ID }
        //                                   where transaction.EMS_SUB_SUB_ID == key
        //                                         && transaction.EMS_PAPER_CODE == paperCode
        //                                         && transaction.EMS_ACTIVITY == activity
        //                                         && transaction.EMS_MARKING_REG_CODE == regionCode
        //                                   select user)
        //                .Include(a => a.Examiner)
        //                .Include(a => a.Role)
        //                .ToListAsync();

        //            return usersList;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"An error occurred: {ex.Message}");
        //            throw;
        //        }
        //    }


        public async Task<IEnumerable<ApplicationUser>> UsersGetAllComponent(string examCode, string subjectCode, string paperCode, string regionCode, string activity)
        {
            try
            {
                List<ApplicationUser> userList = new List<ApplicationUser>();
                if (!string.IsNullOrEmpty(regionCode))
                {
                   
                    var transaction = await _datacontext.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity && a.EMS_MARKING_REG_CODE == regionCode).ToListAsync();


                    foreach (var item in transaction)
                    {
                        var user = await _datacontext.Users
                          .Include(a => a.Examiner)
                          
                          .FirstOrDefaultAsync(a => a.EMS_SUBKEY == item.EMS_SUBKEY && a.IDNumber == item.EMS_NATIONAL_ID);
                        if(user != null)
                        {
                            userList.Add(user);
                        }
                        
                    }
                }
                else
                {
                    var transaction = await _datacontext.EXAMINER_TRANSACTIONS.Where(a => a.EMS_SUB_SUB_ID == examCode + subjectCode && a.EMS_PAPER_CODE == paperCode && a.EMS_ACTIVITY == activity).ToListAsync();


                    foreach (var item in transaction)
                    {
                        var user = await _datacontext.Users
                         .Include(a => a.Examiner)
                       
                         .FirstOrDefaultAsync(a => a.EMS_SUBKEY == item.EMS_SUBKEY && a.IDNumber == item.EMS_NATIONAL_ID);
                        if (user != null)
                        {
                            userList.Add(user);
                        }

                    }
                }
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<IEnumerable<ApplicationUser>> UsersGetAllRegionalMonitors(string examSession, string phase,string regionCode)
        {
            try
            {
                List<ApplicationUser> userList = new List<ApplicationUser>();
                //if (!string.IsNullOrEmpty(centre))
                //{

                //    var transaction = await _datacontext.ExamMonitorTransactions.Where(a => a.Session == examSession  && a.Phase == phase && a.Region == regionCode && a.CentreAttached == centre).ToListAsync();


                //    foreach (var item in transaction)
                //    {
                //        var user = await _datacontext.Users
                //          .Include(a => a.ExamMonitor)

                //          .FirstOrDefaultAsync(a => a.EMS_SUBKEY == item.SubKey && a.IDNumber == item.NationalId);
                //        if (user != null)
                //        {
                //            userList.Add(user);
                //        }

                //    }
                //}
                //else
                //{
                    var transaction = await _datacontext.ExamMonitorTransactions.Where(a => a.Session == examSession && a.Phase == phase && a.Region == regionCode).ToListAsync();


                    foreach (var item in transaction)
                    {
                        var user = await _datacontext.Users
                         .Include(a => a.ExamMonitor)

                         .FirstOrDefaultAsync(a => a.EMS_SUBKEY == item.SubKey && a.IDNumber == item.NationalId);
                        if (user != null)
                        {
                            userList.Add(user);
                        }

                    //}
                }
                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> GetUserById(string Id)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(x => x.Id == Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> GetUserByNationalID(string Id)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(x => x.IDNumber == Id);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<ApplicationUser> SaveUser(ApplicationUser model, string password)
        {
            try
            {
                var result = await _context.CreateAsync(model, password);

                if (result.Succeeded)
                {
                    return model;
                }
                else
                {
                    // Handle the errors in result.Errors
                    throw new ApplicationException("User creation failed");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateUser(ApplicationUser model)
        {
            try
            {
                var userInDb = await _context.FindByIdAsync(model.Id);

                if (userInDb != null)
                {
                    userInDb.EMS_SUBKEY = model.EMS_SUBKEY;
                    userInDb.ExaminerCode = model.ExaminerCode;
                    userInDb.Email = model.Email;
                    userInDb.PhoneNumber = model.PhoneNumber;
                    userInDb.IDNumber = model.IDNumber;
                    userInDb.UserName = model.UserName;
             
                    userInDb.PhoneNumber = model.PhoneNumber;
                  
            


                    var result = await _context.UpdateAsync(userInDb);

                    return result.Succeeded;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DateRange> GetDate()
        {
            try
            {
                return await _datacontext.DateRange.FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> GetUser(string idNumber,string subKey)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(x =>  x.IDNumber == idNumber && x.EMS_SUBKEY == subKey);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> GetUserNationalId(string idNumber)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(x => x.IDNumber == idNumber);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApplicationUser> GetCurrentUser(string idNumber, string subKey,string activity)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(x => x.IDNumber == idNumber && x.EMS_SUBKEY == subKey && x.Activity == activity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteUser(string idNumber,string key)
        {
            var users = await _datacontext.Users.Where(a => a.IDNumber == idNumber).ToListAsync();

            foreach (var user in users)
            {
                if (user != null)
                {
                    // Get all roles for the user
                    var roles = await _context.GetRolesAsync(user);

                    foreach (var role in roles)
                    {
                        await _context.RemoveFromRoleAsync(user, role);
                    }

                    var audits = await _datacontext.AuditTrails.Where(a => a.UserId == user.Id).ToListAsync();

                    foreach (var item in audits)
                    {
                        _datacontext.AuditTrails.Remove(item);
                    }

                    // Now delete the user
                    var result = await _context.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        // Log or handle the errors in result.Errors
                    }
                }



            
            }
        }


        public async Task DeleteUserWithKey(string idNumber, string key)
        {
            var users = await _datacontext.Users.Where(a => a.IDNumber == idNumber && a.EMS_SUBKEY == key).ToListAsync();

            foreach (var user in users)
            {
                if (user != null)
                {
                    // Get all roles for the user
                    var roles = await _context.GetRolesAsync(user);

                    foreach (var role in roles)
                    {
                        await _context.RemoveFromRoleAsync(user, role);
                    }

                    var audits = await _datacontext.AuditTrails.Where(a => a.UserId == user.Id).ToListAsync();

                    foreach (var item in audits)
                    {
                        _datacontext.AuditTrails.Remove(item);
                    }

                    // Now delete the user
                    var result = await _context.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        // Log or handle the errors in result.Errors
                    }
                }




            }
        }
        public async Task<ApplicationUserDTO> SaveZimsecStaffUser(ApplicationUserDTO model,string userId)
        {
            try
            {
                var result =  _datacontext.ZImSecStaff.Add(model);
                await _datacontext.SaveChangesAsync(userId);
                return model;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApplicationUserDTO> GetZimsecStaffById(string Idnumber)
        {
            try
            {
                return await _datacontext.ZImSecStaff.FirstOrDefaultAsync(x => x.IDNumber == Idnumber);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ApplicationUserDTO>> ZimsecStaffGetAll()
        {
            try
            {

                return await _datacontext.ZImSecStaff.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception here or return null, depending on your requirements
                throw;
            }
        }

        public async Task UpdateZimsecStaff(ApplicationUserDTO model)
        {
            var existingEntity = await _datacontext.ZImSecStaff.FirstOrDefaultAsync(c => c.IDNumber == model.IDNumber);
            if (existingEntity != null)
            {
                existingEntity.FirstName= model.FirstName;
                existingEntity.Surname = model.Surname;
                existingEntity.IDNumber = model.IDNumber;
                existingEntity.Mobile = model.Mobile;


                _datacontext.ZImSecStaff.Update(existingEntity);
                await _datacontext.SaveChangesAsync();
            }
        }

        public async Task<OperationResult> AddNewUser(ApplicationUser user, string userId)
        {
            try
            {

                _datacontext.Users.Add(user);
                await _datacontext.SaveChangesAsync(userId);
                return new OperationResult
                {
                    Success = true,
                    Message = "Examiner added successfully"
                };
            }
            catch (DbUpdateException ex)
            {
                // This will give you the exact SQL error
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }
        }
    }
}
