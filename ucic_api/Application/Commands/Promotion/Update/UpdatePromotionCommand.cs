using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Promotion.Update
{
    public class UpdatePromotionCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "PromotionId is required")]
        public long PromotionId { get; set; }

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

    public class UpdatePromotionHandler : IRequestHandler<UpdatePromotionCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Promotion> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Promotion> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdatePromotionHandler(
            ICommandRepository<Domain.Entities.Promotion> commandRepository,
            IQueryRepository<Domain.Entities.Promotion> queryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response<string>> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
        {
            var existingPromotion = await _queryRepository.GetByIdAsync(request.PromotionId);
            if (existingPromotion == null)
            {
                return ResponseFailure("Promotion not found.", "Failed");
            }

            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            existingPromotion.Code = request.Code;
            existingPromotion.CoverageAreaId = request.CoverageAreaId;
            existingPromotion.DiscountPercentage = request.DiscountPercentage;
            existingPromotion.ValidFrom = request.ValidFrom;
            existingPromotion.ValidTo = request.ValidTo;
            existingPromotion.ModifiedDate = DateTime.UtcNow;
            existingPromotion.ModifiedBy = userName;

            try
            {
                await _commandRepository.UpdateAsync(existingPromotion);
                return ResponseSuccess("Successfully updated promotion.", "success");
            }
            catch (Exception)
            {
                return ResponseFailure("Failed to update promotion.", "Failed");
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