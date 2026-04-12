using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using Microsoft.AspNetCore.Http;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Commands.CoverageArea.Delete
{
    public class DeleteCoverageAreaCommand : IRequest<Response<DeleteCoverageAreaCommand>>
    {
        [Required(ErrorMessage = "CoverageAreaId is required")]
        public long CoverageAreaId { get; set; }
    }

    public class DeleteCoverageAreaCommandHandler : IRequestHandler<DeleteCoverageAreaCommand, Response<DeleteCoverageAreaCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.CoverageArea> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.CoverageArea> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DeleteCoverageAreaCommandHandler> _logger;

        public DeleteCoverageAreaCommandHandler(
            ICommandRepository<Domain.Entities.CoverageArea> commandRepository,
            IQueryRepository<Domain.Entities.CoverageArea> queryRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DeleteCoverageAreaCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Response<DeleteCoverageAreaCommand>> Handle(DeleteCoverageAreaCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            var existingCoverageArea = await _queryRepository.GetByIdAsync(request.CoverageAreaId);
            if (existingCoverageArea == null)
            {
                return new Response<DeleteCoverageAreaCommand>
                {
                    Success = false,
                    Message = "Coverage Area not found.",
                    Data = request
                };
            }

            try
            {
                await _commandRepository.DeleteAsync(existingCoverageArea);
                return new Response<DeleteCoverageAreaCommand>
                {
                    Success = true,
                    Message = "Coverage Area deleted successfully.",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coverage area");
                return new Response<DeleteCoverageAreaCommand>
                {
                    Success = false,
                    Message = "Failed to delete the Coverage Area.",
                    Data = request
                };
            }
        }
    }
} 