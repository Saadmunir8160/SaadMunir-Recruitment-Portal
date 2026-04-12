namespace RecruitmentAPI.Domain.Repositories.Query.Base;

public interface IQueryRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id);
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> GetQueryable();
    Task<bool> ExistsAsync(long id);
}
