using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Promotion.Create
{
    public class CreatePromotionCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        public string Code { get; set; }

        [Required(ErrorMessage = "CoverageAreaId is required")]
        public long CoverageAreaId { get; set; }

        [Required(ErrorMessage = "DiscountPercentage is required")]
        public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "ValidFrom is required")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "ValidTo is required")]
        public DateTime ValidTo { get; set; }
    }

    public class CreatePromotionHandler : IRequestHandler<CreatePromotionCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Promotion> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreatePromotionHandler(
            ICommandRepository<Domain.Entities.Promotion> commandRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            var promotion = new Domain.Entities.Promotion
            {
                Code = request.Code,
                CoverageAreaId = request.CoverageAreaId,
                DiscountPercentage = request.DiscountPercentage,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userName,
                IsActive = true
            };

            var result = await _commandRepository.AddAsync(promotion);

            if (result <= 0)
            {
                return ResponseFailure("Failed to create promotion.", "Failed");
            }
            return ResponseSuccess("Successfully created promotion.", "success");
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