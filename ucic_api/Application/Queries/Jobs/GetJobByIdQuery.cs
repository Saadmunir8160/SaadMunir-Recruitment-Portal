using Application.DTOs;
using Application.Queries.News;
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
    public class GetJobByIdQuery : IRequest<JobsDTO>
    {
        public long JobId { get; set; }
    }

    public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobsDTO>
    {
        private readonly IQueryRepository<Domain.Entities.Jobs> _queryRepository;

        public GetJobByIdQueryHandler(IQueryRepository<Domain.Entities.Jobs> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<JobsDTO> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            var jobs = await _queryRepository.GetByIdAsync(request.JobId);
           
            
            if (jobs == null)
                throw new KeyNotFoundException("News not found");

            return new JobsDTO
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
            };
        }
    }
}
