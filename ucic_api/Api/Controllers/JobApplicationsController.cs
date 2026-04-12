using Application.Common.Extensions;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Query.Base;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobApplicationsController : ControllerBase
    {
        private readonly IQueryRepository<JobApplications> _queryRepository;
        public JobApplicationsController(IQueryRepository<JobApplications> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = _queryRepository.GetQueryable();
            var jobApplications = await query
                .Include(j => j.Jobs)
                .ToListAsync();

            var result = jobApplications.Select(j => new JobApplicationDTO
            {
                JobApplicationsId = j.JobApplicationsId,
                FullName = j.FullName,
                Email = j.Email,
                Phone = j.Phone,
                ResumePath = j.ResumePath,
                JobTitle = j.Jobs?.Title ?? string.Empty
            });

            var parameters = new PaginationParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var paginated = await result.ToPaginatedResponseAsync(parameters);
            return Ok(paginated);
        }
    }
} 