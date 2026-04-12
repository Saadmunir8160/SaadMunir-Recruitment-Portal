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

namespace Application.Commands.CoverageArea.Create
{
    public class CreateCoverageAreaCommand : IRequest<Response<CreateCoverageAreaCommand>>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        public string ArabicName { get; set; }
        public string? ImagePath { get; set; }
    }

    public class CreateCoverageAreaCommandHandler : IRequestHandler<CreateCoverageAreaCommand, Response<CreateCoverageAreaCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.CoverageArea> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.CoverageArea> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateCoverageAreaCommandHandler> _logger;

        public CreateCoverageAreaCommandHandler(
            ICommandRepository<Domain.Entities.CoverageArea> commandRepository,
            IQueryRepository<Domain.Entities.CoverageArea> queryRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CreateCoverageAreaCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Response<CreateCoverageAreaCommand>> Handle(CreateCoverageAreaCommand request, CancellationToken cancellationToken)
        {
            // Check if name already exists
            bool nameExists = await _queryRepository.ValueExistsAsync("Name", request.Name);
            if (nameExists)
            {
                return new Response<CreateCoverageAreaCommand>
                {
                    Success = false,
                    Message = $"A coverage area with the name '{request.Name}' already exists.",
                    Data = request
                };
            }

            var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            var item = new Domain.Entities.CoverageArea
            {
                Name = request.Name,
                ArabicName = request.ArabicName,
                ImagePath = request.ImagePath,
                CreatedDate = DateTime.Now,
                CreatedBy = username.IsNullOrEmpty() ? null : username,
            };

            try
            {
                await _commandRepository.AddAsync(item);
                return new Response<CreateCoverageAreaCommand>
                {
                    Success = true,
                    Message = "Coverage Area created successfully.",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coverage area");
                return new Response<CreateCoverageAreaCommand>
                {
                    Success = false,
                    Message = "Failed to create the Coverage Area.",
                    Data = request
                };
            }
        }
    }
}