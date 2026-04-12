using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Jobs.Update
{
    public class UpdateJobsCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Job ID is required")]
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        public long salary { get; set; }
        public long DepartmentId { get; set; }
        public bool IsArabic { get; set; }
        public string jobType { get; set; }
        public string locationType { get; set; }
    }

    public class UpdateJobsHandler : IRequestHandler<UpdateJobsCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Jobs> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Jobs> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateJobsHandler(
            ICommandRepository<Domain.Entities.Jobs> commandRepository,
            IQueryRepository<Domain.Entities.Jobs> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(UpdateJobsCommand request, CancellationToken cancellationToken)
        {
            var existingJob = await _queryRepository.GetByIdAsync(request.Id);
            if (existingJob == null)
            {
                return ResponseFailure("Job not found.", "Failed");
            }

            if (!Enum.TryParse<WorkType>(request.jobType, true, out var workType))
            {
                return ResponseFailure("Invalid job type.", "Failed");
            }

            if (!Enum.TryParse<WorkLocation>(request.locationType, true, out var workLocation))
            {
                return ResponseFailure("Invalid location type.", "Failed");
            }

            existingJob.Title = request.Title;
            existingJob.Description = request.Description;
            existingJob.Location = request.Location;
            existingJob.Department = request.Department;
            existingJob.salary = request.salary;
            existingJob.ModifiedDate = DateTime.UtcNow;
            existingJob.WorkType = workType;
            existingJob.WorkLocation = workLocation;
            existingJob.IsArabic = request.IsArabic;
            await _commandRepository.UpdateAsync(existingJob);

            return ResponseSuccess("Successfully updated Job.", "success");
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