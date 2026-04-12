using Application.DTOs;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Admin.DealerProduct
{
    public class UpdateDealerProductForAdminCommand : IRequest<Response<string>>
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

    public class UpdateDealerProductForAdminCommandHandler : IRequestHandler<UpdateDealerProductForAdminCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerProduct> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;

        public UpdateDealerProductForAdminCommandHandler(
            ICommandRepository<Domain.Entities.DealerProduct> commandRepository,
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<Response<string>> Handle(UpdateDealerProductForAdminCommand request, CancellationToken cancellationToken)
        {
            // Admin can update any product - no dealer check needed
            var existingDealerProduct = await _queryRepository.GetByIdAsync(request.Id);

            if (existingDealerProduct == null)
            {
                return ResponseFailure("Dealer Product not found.", "Failed");
            }

            existingDealerProduct.ProductName = request.ProductName;
            existingDealerProduct.Product_LnCode = request.Product_LnCode;
            existingDealerProduct.Description = request.Description;
            existingDealerProduct.Unit = request.Unit;
            if (request.IsActive.HasValue)
            {
                existingDealerProduct.IsActive = request.IsActive.Value;
            }
            existingDealerProduct.ModifiedDate = DateTime.UtcNow;
            existingDealerProduct.ModifiedBy = "Admin"; // Could be improved to get actual admin user ID

            try
            {
                await _commandRepository.UpdateAsync(existingDealerProduct);
                return ResponseSuccess("Successfully updated dealer product.", "success");
            }
            catch (Exception ex)
            {
                return ResponseFailure($"Failed to update dealer product: {ex.Message}", "Failed");
            }
        }

        private Response<string> ResponseSuccess(string message, string data)
        {
            return new Response<string>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        private Response<string> ResponseFailure(string message, string data)
        {
            return new Response<string>
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}
