using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DealerArea
{
    public class GetAllDealerAreasForAdminQuery : IRequest<PaginatedResponse<DealerAreaForAdminDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAllDealerAreasForAdminQueryHandler : IRequestHandler<GetAllDealerAreasForAdminQuery, PaginatedResponse<DealerAreaForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaRepository;

        public GetAllDealerAreasForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerArea> areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<PaginatedResponse<DealerAreaForAdminDTO>> Handle(GetAllDealerAreasForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _areaRepository.GetQueryable().Where(a => !a.IsDeleted);

            // Apply filters
            if (request.IsActive.HasValue)
            {
                query = query.Where(a => a.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(a => 
                    a.AreaName.ToLower().Contains(searchLower) ||
                    a.AreaCode.ToLower().Contains(searchLower));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var areas = await query
                .OrderBy(a => a.AreaName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var areaDTOs = areas.Select(area => new DealerAreaForAdminDTO
            {
                AreaID = area.AreaID,
                AreaName = area.AreaName,
                AreaCode = area.AreaCode,
                IsActive = area.IsActive,
                CreatedDate = area.CreatedDate,
                ModifiedDate = area.ModifiedDate ?? DateTime.UtcNow,
                OrderCount = 0 // Would need to calculate from DealerOrders
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

            return new PaginatedResponse<DealerAreaForAdminDTO>
            {
                Data = areaDTOs,
                Metadata = new PaginationMetadata
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalRecords,
                    TotalPages = totalPages
                }
            };
        }
    }

    public class GetDealerAreaByIdForAdminQuery : IRequest<DealerAreaForAdminDTO?>
    {
        public int Id { get; set; }
    }

    public class GetDealerAreaByIdForAdminQueryHandler : IRequestHandler<GetDealerAreaByIdForAdminQuery, DealerAreaForAdminDTO?>
    {
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaRepository;

        public GetDealerAreaByIdForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerArea> areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<DealerAreaForAdminDTO?> Handle(GetDealerAreaByIdForAdminQuery request, CancellationToken cancellationToken)
        {
            var area = await _areaRepository.GetByIdAsync(request.Id);
            if (area == null)
                return null;

            return new DealerAreaForAdminDTO
            {
                AreaID = area.AreaID,
                AreaName = area.AreaName,
                AreaCode = area.AreaCode,
                IsActive = area.IsActive,
                CreatedDate = area.CreatedDate,
                ModifiedDate = area.ModifiedDate ?? DateTime.UtcNow,
                OrderCount = 0
            };
        }
    }

    public class GetDealerAreasStatsForAdminQuery : IRequest<DealerAreasStatsForAdminDTO>
    {
    }

    public class GetDealerAreasStatsForAdminQueryHandler : IRequestHandler<GetDealerAreasStatsForAdminQuery, DealerAreasStatsForAdminDTO>
    {
        private readonly IQueryRepository<Domain.Entities.DealerArea> _areaRepository;

        public GetDealerAreasStatsForAdminQueryHandler(IQueryRepository<Domain.Entities.DealerArea> areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<DealerAreasStatsForAdminDTO> Handle(GetDealerAreasStatsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = _areaRepository.GetQueryable().Where(a => !a.IsDeleted);
            var areas = await query.ToListAsync(cancellationToken);

            return new DealerAreasStatsForAdminDTO
            {
                TotalAreas = areas.Count,
                ActiveAreas = areas.Count(a => a.IsActive),
                InactiveAreas = areas.Count(a => !a.IsActive)
            };
        }
    }
}
