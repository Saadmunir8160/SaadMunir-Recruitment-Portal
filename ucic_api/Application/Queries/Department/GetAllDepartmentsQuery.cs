using Application.Common.Extensions;
using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;

namespace Application.Queries.Department
{
    public class GetAllDepartmentsQuery : IRequest<PaginatedResponse<DepartmentDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, PaginatedResponse<DepartmentDto>>
    {
        private readonly IQueryRepository<Domain.Entities.Department> _queryRepository;

        public GetAllDepartmentsQueryHandler(IQueryRepository<Domain.Entities.Department> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Department not found");

            var departmentsQuery = query.Select(dept => new DepartmentDto
            {
                DepartmentId = dept.DepartmentId,
                Name = dept.Name
            });

            return await departmentsQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
