using Application.Commands.JobApplication.Create;
using Application.Commands.Jobs.Create;
using Application.Commands.Jobs.Delete;
using Application.Commands.Jobs.Update;
using Application.Queries.Jobs;
using Application.Queries.Department;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : Controller
    {
        private readonly IMediator _mediator;
        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllJobs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetAllJobsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("GetDepartments")]
        public async Task<ActionResult> GetDepartments()
        {
            return Ok(await _mediator.Send(new GetAllDepartmentsQuery()));
        }

        [HttpPost("ApplyForJob")]
        [RequestSizeLimit(100_000_000)]
        public async Task<ActionResult> CreateJobApplication([FromForm] CreateJobApplicationCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateJobs([FromForm] CreateJobsCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }
        [HttpGet("GetJob/{jobId}")]
        public async Task<ActionResult> GetJob(long jobId)
        {
            return Ok(await _mediator.Send(new GetJobByIdQuery() { JobId = jobId }));
        }

        [HttpDelete("Delete/{jobId}")]
        public async Task<ActionResult> DeleteJob(long jobId)
        {
            return Ok(await _mediator.Send(new DeleteJobCommand() { Id = jobId }));
        }

        [HttpPut("Update/{jobId}")]
        public async Task<ActionResult> UpdateJob(long jobId, [FromForm] UpdateJobsCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            command.Id = jobId;
            return Ok(await _mediator.Send(command));
        }
    }
}
