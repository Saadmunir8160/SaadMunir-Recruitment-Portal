using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories.Query.Base
{
    public interface IQueryRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> GetQueryable();
        Task<IEnumerable<T>> GetByColumnsAsync(Dictionary<string, object> columnFilters);
        Task<bool> ValueExistsAsync(string columnName, object value);
        Task<IEnumerable<T>> GetByColumnsWithListAsync(Dictionary<string, object> columnFilters);
    }
}
