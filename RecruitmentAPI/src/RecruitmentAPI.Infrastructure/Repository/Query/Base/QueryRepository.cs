using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using RecruitmentAPI.Infrastructure.Data;

namespace RecruitmentAPI.Infrastructure.Repository.Query.Base;

public class QueryRepository<T> : IQueryRepository<T> where T : class
{
    protected readonly RecruitmentDbContext _context;

    public QueryRepository(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(long id)
    {
        var entity = await _context.Set<T>().FindAsync(id);

        if (entity is BaseEntity baseEntity && baseEntity.IsDeleted)
            return null;

        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = ApplySoftDeleteFilter(_context.Set<T>().AsQueryable());
        return await query.ToListAsync();
    }

    public IQueryable<T> GetQueryable()
    {
        return _context.Set<T>().AsQueryable();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity is BaseEntity baseEntity)
            return !baseEntity.IsDeleted;
        return entity is not null;
    }

    private static IQueryable<T> ApplySoftDeleteFilter(IQueryable<T> query)
    {
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var falseConstant = Expression.Constant(false);
            var isNotDeleted = Expression.Equal(isDeletedProperty, falseConstant);
            var lambda = Expression.Lambda<Func<T, bool>>(isNotDeleted, parameter);
            return query.Where(lambda);
        }
        return query;
    }
}
