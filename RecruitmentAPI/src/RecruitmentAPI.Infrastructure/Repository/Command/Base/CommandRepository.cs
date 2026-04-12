using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Infrastructure.Data;

namespace RecruitmentAPI.Infrastructure.Repository.Command.Base;

public class CommandRepository<T> : ICommandRepository<T> where T : class
{
    protected readonly RecruitmentDbContext _context;

    public CommandRepository(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<int> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> AddRangeAsync(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
        return await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.ModifiedDate = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }
        else
        {
            _context.Set<T>().Remove(entity);
        }
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(T entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.ModifiedDate = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException($"Entity of type {typeof(T).Name} does not support soft delete.");
        }
    }

    public async Task HardDeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<ITransaction> BeginTransactionAsync()
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        return new EfTransaction(transaction);
    }

    private sealed class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction) => _transaction = transaction;

        public async Task CommitAsync() => await _transaction.CommitAsync();
        public async Task RollbackAsync() => await _transaction.RollbackAsync();
        public void Dispose() => _transaction.Dispose();
    }
}
