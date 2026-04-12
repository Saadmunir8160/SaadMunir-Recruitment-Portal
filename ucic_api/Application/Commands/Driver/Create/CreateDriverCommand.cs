using Domain.Repositories.Command.Base;
using MediatR;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.Driver.Create
{
    public class CreateDriverCommand : IRequest<Response<CreateDriverCommand>>
    {
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

    public class CreateDriverCommandHandler : IRequestHandler<CreateDriverCommand, Response<CreateDriverCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.Driver> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateDriverCommandHandler> _logger;

        public CreateDriverCommandHandler(
            ICommandRepository<Domain.Entities.Driver> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CreateDriverCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Response<CreateDriverCommand>> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

            var driver = new Domain.Entities.Driver
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                IqamaNumber = request.IqamaNumber,
                ErpCode = request.ErpCode,
                UserId = request.UserId,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = username
            };

            var result = await _commandRepository.AddAsync(driver);

            if (result > 0)
            {
                return new Response<CreateDriverCommand>
                {
                    Success = true,
                    Message = "Driver created successfully.",
                    Data = request
                };
            }

            return new Response<CreateDriverCommand>
            {
                Success = false,
                Message = "Failed to create the driver.",
                Data = request
            };
        }
    }
} 