using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.DealerArea
{
    // Query to get all dealer areas in system
    public class GetAllDealerAreasQuery : IRequest<Response<List<DealerAreaDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDealerAreasQueryHandler : IRequestHandler<GetAllDealerAreasQuery, Response<List<DealerAreaDTO>>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerArea> _repository;

        public GetAllDealerAreasQueryHandler(IQueryRepository<Domain.Entities.DealerArea> repository)
        {
            _repository = repository;
        }

        public async Task<Response<List<DealerAreaDTO>>> Handle(GetAllDealerAreasQuery request, CancellationToken cancellationToken)
        {
            var areas = await _repository.GetQueryable()
                .Where(a => !a.IsDeleted)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var result = areas.Select(a => new DealerAreaDTO
            {
                AreaID = a.AreaID,
                AreaName = a.AreaName,
                AreaCode = a.AreaCode,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedDate,
                UpdatedAt = a.ModifiedDate ?? a.CreatedDate
            }).ToList();

            return new Response<List<DealerAreaDTO>>
            {
                Success = true,
                Data = result,
                Message = "Dealer areas retrieved successfully."
            };
        }
    }


    // Query to get single area by id
    public class GetDealerAreaByIdQuery : IRequest<Response<DealerAreaDTO>>
    {
        public int AreaID { get; set; }
    }

    public class GetDealerAreaByIdQueryHandler : IRequestHandler<GetDealerAreaByIdQuery, Response<DealerAreaDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.DealerArea> _repository;

        public GetDealerAreaByIdQueryHandler(IQueryRepository<Domain.Entities.DealerArea> repository)
        {
            _repository = repository;
        }

        public async Task<Response<DealerAreaDTO>> Handle(GetDealerAreaByIdQuery request, CancellationToken cancellationToken)
        {
            var area = await _repository.GetByIdAsync(request.AreaID);
            if (area == null)
            {
                return new Response<DealerAreaDTO>
                {
                    Success = false,
                    Message = "Area not found",
                    Data = null!
                };
            }

            var result = new DealerAreaDTO
            {
                AreaID = area.AreaID,
                AreaName = area.AreaName,
                AreaCode = area.AreaCode,
                IsActive = area.IsActive,
                CreatedAt = area.CreatedDate,
                UpdatedAt = area.ModifiedDate ?? area.CreatedDate
            };

            return new Response<DealerAreaDTO>
            {
                Success = true,
                Data = result,
                Message = "Area retrieved successfully."
            };
        }
    }
}

