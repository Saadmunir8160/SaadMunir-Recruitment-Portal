using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.DepartmentNotificationRecipient.Create
{
    public class CreateEmailNotificationCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string EmailAddress { get; set; }
        public string? PhoneNo { get; set; }

        [Required(ErrorMessage = "Department id is required")]
        public int DepartmentId { get; set; }
    }

    public class CreateEmailNotificationCommandHandler : IRequestHandler<CreateEmailNotificationCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.DepartmentNotificationRecipient> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _imageFileService;

        public CreateEmailNotificationCommandHandler(
            ICommandRepository<Domain.Entities.DepartmentNotificationRecipient> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            IImageFileService imageFileService)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _imageFileService = imageFileService;
        }

        public async Task<Response<string>> Handle(CreateEmailNotificationCommand request, CancellationToken cancellationToken)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            var departmentNotificationRecipient = new Domain.Entities.DepartmentNotificationRecipient
            {
                Name = request.Name,
                EmailAddress = request.EmailAddress,
                PhoneNo = request.PhoneNo,
                Active = true,
                IsActive = true,
                DepartmentId = request.DepartmentId,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };
            var resultPayment = await _commandRepository.AddAsync(departmentNotificationRecipient);

            return ResponseSuccess("Succesfully created email notification.", "success"); ;
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
