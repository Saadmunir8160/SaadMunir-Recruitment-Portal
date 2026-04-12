using Application.Commands.Product.Create;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs;
using Application.Queries.User;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product
{
    public class GetProductsByCoverageAreaQuery : IRequest<Response<List<Domain.Entities.Product>>>
    {
        public long CoverageAreaId { get; set; }
    }

    public class GetProductsByCoverageAreaQueryHandler : IRequestHandler<GetProductsByCoverageAreaQuery, Response<List<Domain.Entities.Product>>>
    {
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;

        private readonly ILogger<GetProductsByCoverageAreaQueryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetProductsByCoverageAreaQueryHandler(IQueryRepository<Domain.Entities.Product> queryRepository, ILogger<GetProductsByCoverageAreaQueryHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _queryRepository = queryRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<List<Domain.Entities.Product>>> Handle(GetProductsByCoverageAreaQuery request, CancellationToken cancellationToken)
        {
            var filters = new Dictionary<string, object>
              {
                  { nameof(Domain.Entities.Product.CoverageAreaId), request.CoverageAreaId }
              };
            var result = await _queryRepository.GetByColumnsAsync(filters);
            if (result == null || !result.Any())
            {
                throw new NotFoundException("No products found.");
            }

            return new Response<List<Domain.Entities.Product>>
            {
                Success = true,
                Message = "Products retrieved successfully.",
                Data = result.ToList()
            };
        }
    }
}

