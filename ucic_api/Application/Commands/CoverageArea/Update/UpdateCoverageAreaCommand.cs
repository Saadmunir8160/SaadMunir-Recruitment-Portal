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

namespace Application.Commands.CoverageArea.Update
{
    public class UpdateCoverageAreaCommand : IRequest<Response<UpdateCoverageAreaCommand>>
    {
        [Required(ErrorMessage = "CoverageAreaId is required")]
        public long CoverageAreaId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        public string ArabicName { get; set; }

        public string? ImagePath { get; set; }
    }

    public class UpdateCoverageAreaCommandHandler : IRequestHandler<UpdateCoverageAreaCommand, Response<UpdateCoverageAreaCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.CoverageArea> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.CoverageArea> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UpdateCoverageAreaCommandHandler> _logger;

        public UpdateCoverageAreaCommandHandler(
            ICommandRepository<Domain.Entities.CoverageArea> commandRepository,
            IQueryRepository<Domain.Entities.CoverageArea> queryRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UpdateCoverageAreaCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Response<UpdateCoverageAreaCommand>> Handle(UpdateCoverageAreaCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            var existingCoverageArea = await _queryRepository.GetByIdAsync(request.CoverageAreaId);
            if (existingCoverageArea == null)
            {
                return new Response<UpdateCoverageAreaCommand>
                {
                    Success = false,
                    Message = "Coverage Area not found.",
                    Data = request
                };
            }

            // Check if name or arabic name already exists (excluding current coverage area)
            bool nameExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.CoverageArea.Name), request.Name);
            bool arabicNameExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.CoverageArea.ArabicName), request.ArabicName);

            if (nameExists && arabicNameExists)
            {
                return new Response<UpdateCoverageAreaCommand>
                {
                    Success = false,
                    Message = $"A coverage area with the name '{request.Name}' or Arabic name '{request.ArabicName}' already exists.",
                    Data = request
                };
            }

            existingCoverageArea.Name = request.Name;
            existingCoverageArea.ArabicName= request.ArabicName;
            existingCoverageArea.ImagePath = request.ImagePath;
            existingCoverageArea.ModifiedDate = DateTime.Now;
            existingCoverageArea.ModifiedBy = username.IsNullOrEmpty() ? null : username;

            try
            {
                await _commandRepository.UpdateAsync(existingCoverageArea);
                return new Response<UpdateCoverageAreaCommand>
                {
                    Success = true,
                    Message = "Coverage Area updated successfully.",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coverage area");
                return new Response<UpdateCoverageAreaCommand>
                {
                    Success = false,
                    Message = "Failed to update the Coverage Area.",
                    Data = request
                };
            }
        }
    }
} 