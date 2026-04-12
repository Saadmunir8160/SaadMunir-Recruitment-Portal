using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.DepartmentNotificationRecipient.Update
{
    public class UpdateDepartmentNotificationRecipientCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "ID is required")]
        public long Id { get; set; }

        [Required(ErrorMessage = "IsActive is required")]
        public bool IsActive { get; set; }
    }

    public class UpdateDepartmentNotificationRecipientCommandHandler : IRequestHandler<UpdateDepartmentNotificationRecipientCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.DepartmentNotificationRecipient> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateDepartmentNotificationRecipientCommandHandler(
            ICommandRepository<Domain.Entities.DepartmentNotificationRecipient> commandRepository,
            IQueryRepository<Domain.Entities.DepartmentNotificationRecipient> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(UpdateDepartmentNotificationRecipientCommand request, CancellationToken cancellationToken)
        {

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var existingJob = await _queryRepository.GetByIdAsync(request.Id);
            if (existingJob == null)
            {
                return ResponseFailure("Record not found.", "Failed");
            }

            existingJob.Active = request.IsActive;
            existingJob.ModifiedDate = DateTime.UtcNow;
            existingJob.ModifiedBy = userName;

            await _commandRepository.UpdateAsync(existingJob); // UpdateAsync returns void, so no assignment is needed.

            return ResponseSuccess("Successfully updated department Notification Recipient.", "success");
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