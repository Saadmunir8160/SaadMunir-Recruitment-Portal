using Application.DTOs;
using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.DealerProduct.Update
{
    public class UpdateDealerProductCommand : IRequest<Response<string>>
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "ProductName is required")]
        [StringLength(200, ErrorMessage = "ProductName cannot exceed 200 characters")]
        public required string ProductName { get; set; }

        [StringLength(100, ErrorMessage = "Product_LnCode cannot exceed 100 characters")]
        public string? Product_LnCode { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(10, ErrorMessage = "Unit cannot exceed 10 characters")]
        public string Unit { get; set; } = "bags";
        
        public bool? IsActive { get; set; }
    }

    public class UpdateDealerProductCommandHandler : IRequestHandler<UpdateDealerProductCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerProduct> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;
        private readonly IIdentityService _identityService;

        public UpdateDealerProductCommandHandler(
            ICommandRepository<Domain.Entities.DealerProduct> commandRepository,
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository,
            IIdentityService identityService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
        }

        public async Task<Response<string>> Handle(UpdateDealerProductCommand request, CancellationToken cancellationToken)
        {
            // Get current dealer ID
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId == null)
            {
                return ResponseFailure("Dealer not found for current user.", "Failed");
            }

            // Find product that belongs to current dealer
            var existingDealerProduct = await _queryRepository.GetQueryable()
                .Where(p => p.DealerProductID == request.Id && p.DealerID == currentDealerId.Value)
                .FirstOrDefaultAsync();

            if (existingDealerProduct == null)
            {
                return ResponseFailure("Dealer Product not found.", "Failed");
            }

            existingDealerProduct.ProductName = request.ProductName;
            existingDealerProduct.Product_LnCode = request.Product_LnCode;
            existingDealerProduct.Description = request.Description;
            existingDealerProduct.Unit = request.Unit; // Update the unit
            if (request.IsActive.HasValue)
            {
                existingDealerProduct.IsActive = request.IsActive.Value;
            }
            existingDealerProduct.ModifiedDate = DateTime.UtcNow;
            existingDealerProduct.ModifiedBy = _identityService.GetCurrentUserId();

            try
            {
                await _commandRepository.UpdateAsync(existingDealerProduct);
                return ResponseSuccess("Successfully updated dealer product.", "success");
            }
            catch (Exception)
            {
                return ResponseFailure("Failed to update dealer product.", "Failed");
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