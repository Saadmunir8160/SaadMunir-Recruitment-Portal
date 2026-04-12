using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories.Command.Base
{
    public interface ICommandRepository<T> where T : class
    {
        Task<int> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SoftDeleteAsync(T entity);
        Task HardDeleteAsync(T entity);
        Task<int> AddRangeAsync(IEnumerable<T> entities);
        Task<int> UpdateColumnAsync(object primaryKeyValue, string columnName, object value);
        Task<ITransaction> BeginTransactionAsync();
    }

    public interface ITransaction : IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
