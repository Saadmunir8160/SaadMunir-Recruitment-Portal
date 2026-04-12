using Domain.Repositories.Command.Base;
using MediatR;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.Transporter.Create
{
    public class CreateTransporterCommand : IRequest<Response<CreateTransporterCommand>>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string? Code { get; set; }

        [MaxLength(100)]
        public string? ErpCode { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }
    }

    public class CreateTransporterCommandHandler : IRequestHandler<CreateTransporterCommand, Response<CreateTransporterCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.Transporter> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateTransporterCommandHandler> _logger;

        public CreateTransporterCommandHandler(
            ICommandRepository<Domain.Entities.Transporter> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CreateTransporterCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Response<CreateTransporterCommand>> Handle(CreateTransporterCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

            var transporter = new Domain.Entities.Transporter
            {
                Name = request.Name,
                Code = request.Code,
                ErpCode = request.ErpCode,
                UserId = request.UserId,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = username
            };

            var result = await _commandRepository.AddAsync(transporter);

            if (result > 0)
            {
                return new Response<CreateTransporterCommand>
                {
                    Success = true,
                    Message = "Transporter created successfully.",
                    Data = request
                };
            }

            return new Response<CreateTransporterCommand>
            {
                Success = false,
                Message = "Failed to create the transporter.",
                Data = request
            };
        }
    }
} 