using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AutoMapper;

namespace Application.Queries.CoverageArea
{
    public class GetAllCoverageAreasQuery : IRequest<PaginatedResponse<CoverageAreaResponseDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllCoverageAreasQueryHandler : IRequestHandler<GetAllCoverageAreasQuery, PaginatedResponse<CoverageAreaResponseDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.CoverageArea> _queryRepository;
        private readonly IMapper _mapper;

        public GetAllCoverageAreasQueryHandler(IQueryRepository<Domain.Entities.CoverageArea> queryRepository, IMapper mapper)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<CoverageAreaResponseDTO>> Handle(GetAllCoverageAreasQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
            {
                throw new NotFoundException("No coverage areas found.");
            }

            var coverageAreasQuery = query.Select(area => _mapper.Map<CoverageAreaResponseDTO>(area));
            return await coverageAreasQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
