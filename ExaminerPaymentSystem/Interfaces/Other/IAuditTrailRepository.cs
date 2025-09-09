using ExaminerPaymentSystem.Models.Common;

namespace ExaminerPaymentSystem.Interfaces.Other
{
    public interface IAuditTrailRepository
    {
        Task<AuditTrail> GetAuditTrailById(int Id);

        Task<AuditTrail> GetAuditTrailByNameID(string name);




        Task<AuditTrail> SaveAuditTrail(AuditTrail model);
        Task UpdateAuditTrail(AuditTrail model);
        Task<IEnumerable<AuditTrail>> AuditTrailGetAll();
    }
}
