namespace ExaminerPaymentSystem.Interfaces.Common
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> InsertAsync(T entity);
        Task UpdateAsycn(T entity);
        Task DeleteAsync(int id);

    }
}
