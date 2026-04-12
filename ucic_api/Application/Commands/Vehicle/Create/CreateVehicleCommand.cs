using Domain.Repositories.Command.Base;
using MediatR;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.Vehicle.Create
{
    public class CreateVehicleCommand : IRequest<Response<CreateVehicleCommand>>
    {
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

    public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Response<CreateVehicleCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.Vehicle> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateVehicleCommandHandler> _logger;

        public CreateVehicleCommandHandler(
            ICommandRepository<Domain.Entities.Vehicle> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CreateVehicleCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Response<CreateVehicleCommand>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

                var vehicle = new Domain.Entities.Vehicle
                {
                    Name = request.Name,
                    Type = request.Type,
                    RegistrationNo = request.RegistrationNo,
                    Model = request.Model,
                    UserId = request.UserId,
                    ErpCode = request.ErpCode,
                    CreatedDate = DateTime.Now,
                    CreatedBy = username
                };

                var result = await _commandRepository.AddAsync(vehicle);

                if (result > 0)
                {
                    return new Response<CreateVehicleCommand>
                    {
                        Success = true,
                        Message = "Vehicle created successfully.",
                        Data = request
                    };
                }

                return new Response<CreateVehicleCommand>
                {
                    Success = false,
                    Message = "Failed to create the vehicle.",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the vehicle.");
                return new Response<CreateVehicleCommand>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Data = request
                };
            }
        }
    }
} 