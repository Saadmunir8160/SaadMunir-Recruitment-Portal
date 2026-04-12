using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Driver.Update
{
    public class UpdateDriverCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [StringLength(50, ErrorMessage = "Iqama Number cannot exceed 50 characters")]
        public string? IqamaNumber { get; set; }

        [MaxLength(100)]
        public string? ErpCode { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }
    }

    public class UpdateDriverCommandHandler : IRequestHandler<UpdateDriverCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Driver> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Driver> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateDriverCommandHandler(
            ICommandRepository<Domain.Entities.Driver> commandRepository,
            IQueryRepository<Domain.Entities.Driver> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
        {
            var existingDriver = await _queryRepository.GetByIdAsync(request.Id);
            if (existingDriver == null)
            {
                return ResponseFailure("Driver not found.", "Failed");
            }

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            existingDriver.Name = request.Name;
            existingDriver.PhoneNumber = request.PhoneNumber;
            existingDriver.IqamaNumber = request.IqamaNumber;
            existingDriver.ErpCode = request.ErpCode;
            existingDriver.UserId = request.UserId;
            existingDriver.ModifiedDate = DateTime.UtcNow;
            existingDriver.ModifiedBy = userName;

            try
            {
                await _commandRepository.UpdateAsync(existingDriver);
                return ResponseSuccess("Successfully updated driver.", "success");
            }
            catch (Exception)
            {
                return ResponseFailure("Failed to update driver.", "Failed");
            }
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