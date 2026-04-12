using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Jobs.Create
{
    public class CreateJobsCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Location is required")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string? Department { get; set; }
        [Required(ErrorMessage = "salary is required")]
        public long salary { get; set; }
        public long DepartmentId { get; set; }
        public bool IsArabic { get; set; }
        public string jobType { get; set; }
        public string locationType { get; set; }
    }

    public class CreateJobsHandler : IRequestHandler<CreateJobsCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Jobs> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _imageFileService;

        public CreateJobsHandler(
            ICommandRepository<Domain.Entities.Jobs> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            IImageFileService imageFileService)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _imageFileService = imageFileService;
        }

        public async Task<Response<string>> Handle(CreateJobsCommand request, CancellationToken cancellationToken)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            if (!Enum.TryParse<WorkType>(request.jobType, true, out var workType))
            {
                return ResponseFailure("Invalid job type.", "Failed");
            }

            if (!Enum.TryParse<WorkLocation>(request.locationType, true, out var workLocation))
            {
                return ResponseFailure("Invalid location type.", "Failed");
            }

            var job = new Domain.Entities.Jobs
            {
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                CreatedDate = DateTime.UtcNow,
                Department = request.Department,
                DepartmentId = request.DepartmentId,
                salary = request.salary,
                IsArabic = request.IsArabic,
                WorkType = workType,
                WorkLocation = workLocation
            };

            var createdJob = await _commandRepository.AddAsync(job);

            if (createdJob <= 0)
            {
                return ResponseFailure("Failed to create Job.", "Failed");
            }
            return ResponseSuccess("Successfully created Job.", "success");
        }

        private Response<string> ResponseSuccess(string message, string request)
        {
            return new Response<string>
            {
                Success = true,
                Message = message,
                Data = request
            };
        }

        private Response<string> ResponseFailure(string message, string request)
        {
            return new Response<string>
            {
                Success = false,
                Message = message,
                Data = request
            };
        }
    }
}
