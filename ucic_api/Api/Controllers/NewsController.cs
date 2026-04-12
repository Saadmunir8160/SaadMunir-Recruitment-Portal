using Application.Commands.News.Create;
using Application.Commands.News.Delete;
using Application.Commands.News.Update;
using Application.Queries.News;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : Controller
    {
        private readonly IMediator _mediator;
        public NewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllNews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetNewsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateNews([FromForm] CreateNewsCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetNews/{newsId}")]
        public async Task<ActionResult> GetNews(long newsId)
        {
            return Ok(await _mediator.Send(new GetNewsByIdQuery() { NewsId = newsId }));
        }

        [HttpPut("Update/{newsId}")]
        public async Task<ActionResult> UpdateNews(long newsId, [FromForm] UpdateNewsCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            command.NewsId = newsId;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Delete/{newsId}")]
        public async Task<ActionResult> DeleteJob(long newsId)
        {
            return Ok(await _mediator.Send(new DeleteNewsCommand() { Id = newsId }));
        }
    }
}
