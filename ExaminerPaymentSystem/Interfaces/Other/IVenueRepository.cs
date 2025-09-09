using ExaminerPaymentSystem.Data;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IVenueRepository
    {
        Task<Venue> GetVenueById(int Id);

        Task<Venue> GetVenueByNameID(string name);


        Task ActivateVenue(Venue model);

        Task<Venue> SaveVenue(Venue model);
        Task UpdateVenue(Venue model);
        Task<IEnumerable<Venue>> VenuesGetAll();

        Task<SubjectVenue> GetVenueSUbject(string subject, string papercode);
    }
}
