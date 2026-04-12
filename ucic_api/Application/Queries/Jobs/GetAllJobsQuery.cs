using Application.Common.Extensions;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Jobs
{
    public class GetAllJobsQuery : IRequest<PaginatedResponse<JobsDTO>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, PaginatedResponse<JobsDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Jobs> _queryRepository;

        public GetAllJobsQueryHandler(IQueryRepository<Domain.Entities.Jobs> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<PaginatedResponse<JobsDTO>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
        {
            var parameters = new PaginationParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = _queryRepository.GetQueryable();
            if (query == null || !query.Any())
                throw new KeyNotFoundException("Jobs not found");

            var jobsQuery = query
                .OrderByDescending(jobs => jobs.JobsId)
                .Select(jobs => new JobsDTO
                {
                    JobsId = jobs.JobsId,
                    Title = jobs.Title,
                    Description = jobs.Description,
                    Location = jobs.Location,
                    DepartmentId = jobs.DepartmentId,
                    PostedDate = jobs.PostedDate,
                    WorkType = jobs.WorkType.ToString(),
                    WorkLocation = jobs.WorkLocation.ToString(),
                    Salary = jobs.salary,
                    IsArabic = jobs.IsArabic
                });

            return await jobsQuery.ToPaginatedResponseAsync(parameters);
        }
    }
}
