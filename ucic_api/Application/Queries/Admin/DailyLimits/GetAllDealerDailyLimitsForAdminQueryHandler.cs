using MediatR;
using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Query.Base;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DailyLimits
{
    public class GetAllDealerDailyLimitsForAdminQueryHandler : IRequestHandler<GetAllDealerDailyLimitsForAdminQuery, Response<object>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerDailyLimit> _dailyLimitRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly ILogger<GetAllDealerDailyLimitsForAdminQueryHandler> _logger;

        public GetAllDealerDailyLimitsForAdminQueryHandler(
            IQueryRepository<Domain.Entities.DealerDailyLimit> dailyLimitRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            ILogger<GetAllDealerDailyLimitsForAdminQueryHandler> logger)
        {
            _dailyLimitRepository = dailyLimitRepository;
            _dealerRepository = dealerRepository;
            _logger = logger;
        }

        public async Task<Response<object>> Handle(GetAllDealerDailyLimitsForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _dailyLimitRepository.GetQueryable();

                // Apply dealer filter
                if (request.DealerId.HasValue)
                {
                    query = query.Where(dl => dl.DealerID == request.DealerId.Value);
                }

                // Apply limit type filter
                if (!string.IsNullOrEmpty(request.LimitType))
                {
                    query = query.Where(dl => dl.LimitType == request.LimitType);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(dl => 
                        (dl.Dealer != null && dl.Dealer.DealerName.Contains(request.Search)) ||
                        dl.LimitType!.Contains(request.Search));
                }

                // Include dealer information
                query = query.Include(dl => dl.Dealer);

                // Get total count
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var dailyLimits = await query
                    .OrderByDescending(dl => dl.CreatedDate)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(dl => new
                    {
                        dl.DailyLimitID,
                        dl.DealerID,
                        DealerName = dl.Dealer != null ? dl.Dealer.DealerName : "General (All Dealers)",
                        dl.LimitType,
                        dl.LimitValue,
                        dl.EffectiveDate,
                        dl.IsActive,
                        dl.CreatedDate,
                        UpdatedDate = dl.ModifiedDate
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                var result = new
                {
                    Data = dailyLimits,
                    Metadata = new
                    {
                        CurrentPage = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNext = request.PageNumber < totalPages,
                        HasPrevious = request.PageNumber > 1
                    }
                };

                return new Response<object>
                {
                    Success = true,
                    Data = result,
                    Message = "Daily limits retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dealer daily limits for admin");
                return new Response<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving daily limits"
                };
            }
        }
    }
}