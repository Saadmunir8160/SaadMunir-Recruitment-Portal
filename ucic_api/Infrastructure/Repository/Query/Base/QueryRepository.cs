using Domain.Entities;
using Domain.Repositories.Query.Base;
using Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infra.Repository.Query.Base
{
    public class QueryRepository<T> : IQueryRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public QueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<T> ApplySoftDeleteFilter(IQueryable<T> query)
        {
            // If T inherits from BaseEntity, filter out soft-deleted records
            if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
            {
                var parameter = Expression.Parameter(typeof(T), "entity");
                var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
                var falseConstant = Expression.Constant(false);
                var isNotDeleted = Expression.Equal(isDeletedProperty, falseConstant);
                var lambda = Expression.Lambda<Func<T, bool>>(isNotDeleted, parameter);
                
                return query.Where(lambda);
            }
            
            return query;
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            if (typeof(T) == typeof(Domain.Entities.Vendor))
            {
                var query = _context.Set<T>()
                    .Include("Currency")
                    .Include("Country")
                    .Include("CitiesByCountry");
                
                query = ApplySoftDeleteFilter(query);
                return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, "VendorId").Equals(id));
            }

            if (typeof(T) == typeof(DepartmentNotificationRecipient))
            {
                var query = _context.Set<T>().Include("Department");
                query = ApplySoftDeleteFilter(query);
                return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));
            }

            if (typeof(T) == typeof(Domain.Entities.Order))
            {
                IQueryable<T> query = (IQueryable<T>)(object)_context.Set<Order>()
                    .Include(o => o.Customer)
                    .Include(o => o.Location)
                    .Include(o => o.GpsLocation)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product);
                
                query = ApplySoftDeleteFilter(query);
                return await query.FirstOrDefaultAsync(e => ((Order)(object)e).OrderId.Equals(id));
            }

            IQueryable<T> baseQuery = _context.Set<T>();
            baseQuery = ApplySoftDeleteFilter(baseQuery);
            return await baseQuery.FirstOrDefaultAsync(e => EF.Property<object>(e, GetPrimaryKeyPropertyName()).Equals(id));
        }

        private string GetPrimaryKeyPropertyName()
        {
            // Dealer-related tables
            if (typeof(T) == typeof(Domain.Entities.Dealer))
                return "DealerId";
            if (typeof(T) == typeof(Domain.Entities.DealerArea))
                return "AreaID";
            if (typeof(T) == typeof(Domain.Entities.DealerProduct))
                return "DealerProductID";
            if (typeof(T) == typeof(Domain.Entities.DealerOrder))
                return "DealerOrderID";
            if (typeof(T) == typeof(Domain.Entities.DealerOrderItem))
                return "OrderItemID";
            if (typeof(T) == typeof(Domain.Entities.DealerDriver))
                return "DriverID";
            if (typeof(T) == typeof(Domain.Entities.DealerVehicle))
                return "VehicleID";
            if (typeof(T) == typeof(Domain.Entities.DealerShippingAddress))
                return "AddressID";
            if (typeof(T) == typeof(Domain.Entities.DealerDailyLimit))
                return "DailyLimitID";
            // Support Ticket related entities
            if (typeof(T) == typeof(Domain.Entities.SupportTicket))
                return "TicketId";
            if (typeof(T) == typeof(Domain.Entities.SupportTicketMessage))
                return "MessageId";
            // Existing mappings
            if (typeof(T) == typeof(Domain.Entities.Product))
                return "ProductId";
            if (typeof(T) == typeof(Domain.Entities.Jobs))
                return "JobsId";
            if (typeof(T) == typeof(Domain.Entities.News))
                return "NewsId";
            if (typeof(T) == typeof(Domain.Entities.Vendor))
                return "VendorId";
            if (typeof(T) == typeof(Domain.Entities.Promotion))
                return "PromotionId";
            if (typeof(T) == typeof(Domain.Entities.CoverageArea))
                return "CoverageAreaId";
            if (typeof(T) == typeof(Domain.Entities.DepartmentNotificationRecipient))
                return "Id";
            if (typeof(T) == typeof(Domain.Entities.Order))
                return "OrderId";
            if (typeof(T) == typeof(Domain.Entities.Vehicle))
                return "VehicleId";
            if (typeof(T) == typeof(Domain.Entities.Driver))
                return "Id";
            if (typeof(T) == typeof(Domain.Entities.Transporter))
                return "Id";
            // Default fallback
            return "Id";
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            IQueryable<T> query = _context.Set<T>();

            if (typeof(T) == typeof(Order))
            {
                query = (IQueryable<T>)(object)_context.Set<Order>()
                    .Include(o => o.Customer)
                    .Include(o => o.Location)
                    .Include(o => o.GpsLocation)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product);
            }

            if (typeof(T) == typeof(Product))
            {
                query = (IQueryable<T>)(object)_context.Set<Product>()
                    .Include(o => o.CoverageArea);
            }

            if (typeof(T) == typeof(DepartmentNotificationRecipient))
            {
                query = (IQueryable<T>)(object)_context.Set<DepartmentNotificationRecipient>()
                    .Include(o => o.Department);
            }
            
            if (typeof(T) == typeof(Vendor))
            {
                query = (IQueryable<T>)(object)_context.Set<Vendor>()
                    .Include(o => o.Category);
            }

            query = ApplySoftDeleteFilter(query);
            return await query.ToListAsync();
        }

        public IQueryable<T> GetQueryable()
        {
            var query = _context.Set<T>();
            return ApplySoftDeleteFilter(query);
        }

        public async Task<IEnumerable<T>> GetByColumnsAsync(Dictionary<string, object> columnFilters)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var filter in columnFilters)
            {
                var parameter = Expression.Parameter(typeof(T), "entity");
                var property = Expression.Property(parameter, filter.Key);
                var constant = Expression.Constant(filter.Value);
                var equality = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

                query = query.Where(lambda);
            }

            query = ApplySoftDeleteFilter(query);
            return await query.ToListAsync();
        }

        public async Task<bool> ValueExistsAsync(string columnName, object value)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            var property = Expression.Property(parameter, columnName);
            var constant = Expression.Constant(value);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            var query = _context.Set<T>().Where(lambda);
            query = ApplySoftDeleteFilter(query);
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<T>> GetByColumnsWithListAsync(Dictionary<string, object> columnFilters)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var filter in columnFilters)
            {
                var parameter = Expression.Parameter(typeof(T), "entity");
                var property = Expression.Property(parameter, filter.Key);
                var propertyValue = filter.Value;

                if (propertyValue is IEnumerable<long> list)
                {
                    // Create a Contains method for the list of ProductIds
                    var containsMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(property.Type);

                    var constant = Expression.Constant(propertyValue);
                    var containsExpression = Expression.Call(null, containsMethod, constant, property);
                    var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

                    query = query.Where(lambda);
                }
                else
                {
                    // Default equality check for single value
                    var constant = Expression.Constant(propertyValue);
                    var equality = Expression.Equal(property, constant);
                    var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

                    query = query.Where(lambda);
                }
            }

            query = ApplySoftDeleteFilter(query);
            return await query.ToListAsync();
        }
    }
}
