using Domain.Entities;
using Domain.Repositories.Command.Base;
using Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repository.Command.Base
{
    public class CommandRepository<T> : ICommandRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public CommandRepository(ApplicationDbContext context)
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
            // Check if the entity inherits from BaseEntity (has IsDeleted property)
            if (entity is BaseEntity baseEntity)
            {
                // Implement soft delete
                baseEntity.IsDeleted = true;
                baseEntity.ModifiedDate = DateTime.Now;
                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                // Hard delete for entities that don't inherit from BaseEntity
                _context.Set<T>().Remove(entity);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(T entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.ModifiedDate = DateTime.Now;
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException($"Entity of type {typeof(T).Name} does not support soft delete as it doesn't inherit from BaseEntity.");
            }
        }

        public async Task HardDeleteAsync(T entity)
        {
            // Physically remove the entity from the database
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateColumnAsync(object primaryKeyValue, string columnName, object value)
        {
            // Find the entity using the primary key
            var entity = await _context.Set<T>().FindAsync(primaryKeyValue);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with primary key '{primaryKeyValue}' was not found.");
            }

            // Use reflection to set the value of the specified column
            var property = typeof(T).GetProperty(columnName);
            if (property == null || !property.CanWrite)
            {
                throw new ArgumentException($"The column '{columnName}' does not exist or is not writable.");
            }

            property.SetValue(entity, value);

            // Mark the specific property as modified
            _context.Entry(entity).Property(columnName).IsModified = true;

            // Save changes to the database
            return await _context.SaveChangesAsync();
        }

        public async Task<ITransaction> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return new EfTransaction(transaction); 
        }
    }

    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }
    }
}
