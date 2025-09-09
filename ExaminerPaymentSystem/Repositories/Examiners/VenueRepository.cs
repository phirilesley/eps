using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;
using Microsoft.EntityFrameworkCore;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class VenueRepository : IVenueRepository
    {
        private readonly ApplicationDbContext _context;
        public VenueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ActivateVenue(Venue model)
        {
            var existingEntity = await _context.VENUES.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (existingEntity != null)
            {
            
                existingEntity.Status = model.Status;


                _context.VENUES.Update(existingEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Venue> GetVenueById(int Id)
        {
            try
            {
                return await _context.VENUES.FirstOrDefaultAsync(x => x.Id == Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Venue> GetVenueByNameID(string name)
        {
            try
            {
                return await _context.VENUES.FirstOrDefaultAsync(x => x.Name == name);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public  async Task<SubjectVenue> GetVenueSUbject(string subject, string papercode)
        {
            var subjectV = subject.Substring(3);
            subjectV = subjectV;
            var existingEntity = await _context.SubjectVenue.FirstOrDefaultAsync(c => c.Subject == subjectV && c.PaperCode == papercode);
            return existingEntity;
        }

        public async Task<Venue> SaveVenue(Venue model)
        {
            _context.VENUES.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateVenue(Venue model)
        {
            var existingEntity = await _context.VENUES.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (existingEntity != null)
            {
                existingEntity.Name = model.Name;
                existingEntity.Status = model.Status;
          

                _context.VENUES.Update(existingEntity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Venue>> VenuesGetAll()
        {
            try
            {

                return await _context.VENUES.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                // You might want to throw the exception here or return null, depending on your requirements
                throw;
            }
        }
    }
}
