using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Vehicle.Update
{
    public class UpdateVehicleCommand : IRequest<Response<string>>
    {
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        public string? Type { get; set; }

        [Required(ErrorMessage = "Registration Number is required")]
        [StringLength(50, ErrorMessage = "Registration Number cannot exceed 50 characters")]
        public string RegistrationNo { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(100)]
        public string? ErpCode { get; set; }
    }

    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Vehicle> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Vehicle> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateVehicleCommandHandler(
            ICommandRepository<Domain.Entities.Vehicle> commandRepository,
            IQueryRepository<Domain.Entities.Vehicle> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var existingVehicle = await _queryRepository.GetByIdAsync(request.VehicleId);
            if (existingVehicle == null)
            {
                return ResponseFailure("Vehicle not found.", "Failed");
            }

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            existingVehicle.Name = request.Name;
            existingVehicle.Type = request.Type;
            existingVehicle.RegistrationNo = request.RegistrationNo;
            existingVehicle.Model = request.Model;
            existingVehicle.UserId = request.UserId;
            existingVehicle.ErpCode = request.ErpCode;
            existingVehicle.ModifiedDate = DateTime.UtcNow;
            existingVehicle.ModifiedBy = userName;

            try
            {
                await _commandRepository.UpdateAsync(existingVehicle);
                return ResponseSuccess("Successfully updated vehicle.", "success");
            }
            catch (Exception)
            {
                return ResponseFailure("Failed to update vehicle.", "Failed");
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