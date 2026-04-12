using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Transporter.Update
{
    public class UpdateTransporterCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }

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

    public class UpdateTransporterCommandHandler : IRequestHandler<UpdateTransporterCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Transporter> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Transporter> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateTransporterCommandHandler(
            ICommandRepository<Domain.Entities.Transporter> commandRepository,
            IQueryRepository<Domain.Entities.Transporter> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(UpdateTransporterCommand request, CancellationToken cancellationToken)
        {
            var existingTransporter = await _queryRepository.GetByIdAsync(request.Id);
            if (existingTransporter == null)
            {
                return ResponseFailure("Transporter not found.", "Failed");
            }

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            existingTransporter.Name = request.Name;
            existingTransporter.Code = request.Code;
            existingTransporter.ErpCode = request.ErpCode;
            existingTransporter.UserId = request.UserId;
            existingTransporter.ModifiedDate = DateTime.UtcNow;
            existingTransporter.ModifiedBy = userName;

            try
            {
                await _commandRepository.UpdateAsync(existingTransporter);
                return ResponseSuccess("Successfully updated transporter.", "success");
            }
            catch (Exception)
            {
                return ResponseFailure("Failed to update transporter.", "Failed");
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