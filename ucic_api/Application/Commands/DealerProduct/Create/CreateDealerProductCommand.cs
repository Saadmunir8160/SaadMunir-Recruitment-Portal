using Application.Common.Interfaces;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Commands.DealerProduct.Create
{
    public class CreateDealerProductCommand : IRequest<Response<CreateDealerProductCommand>>
    {
        [Required(ErrorMessage = "ProductName is required")]
        [StringLength(200, ErrorMessage = "ProductName cannot exceed 200 characters")]
        public required string ProductName { get; set; }

        [StringLength(100, ErrorMessage = "Product_LnCode cannot exceed 100 characters")]
        public string? Product_LnCode { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(10, ErrorMessage = "Unit cannot exceed 10 characters")]
        public string Unit { get; set; } = "BAG";
    }

    public class CreateDealerProductCommandHandler : IRequestHandler<CreateDealerProductCommand, Response<CreateDealerProductCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerProduct> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _queryRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<CreateDealerProductCommandHandler> _logger;

        public CreateDealerProductCommandHandler(
            ICommandRepository<Domain.Entities.DealerProduct> commandRepository,
            IQueryRepository<Domain.Entities.DealerProduct> queryRepository,
            IIdentityService identityService,
            ILogger<CreateDealerProductCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<Response<CreateDealerProductCommand>> Handle(CreateDealerProductCommand request, CancellationToken cancellationToken)
        {
            // Try to get current dealer ID, but it's now optional
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();

            if (!string.IsNullOrEmpty(request.Product_LnCode))
            {
                var productExists = await _queryRepository.ValueExistsAsync(nameof(Domain.Entities.DealerProduct.Product_LnCode), request.Product_LnCode);
                if (productExists)
                {
                    return new Response<CreateDealerProductCommand>
                    {
                        Success = false,
                        Message = $"Product LN Code '{request.Product_LnCode}' already exists.",
                        Data = request
                    };
                }
            }

            var dealerProduct = new Domain.Entities.DealerProduct
            {
                DealerID = currentDealerId, // Can be null now
                ProductName = request.ProductName,
                Product_LnCode = request.Product_LnCode,
                Description = request.Description,
                Unit = request.Unit, // Set the unit from request
                CreatedDate = DateTime.Now,
                CreatedBy = _identityService.GetCurrentUserId()
            };

            var result = await _commandRepository.AddAsync(dealerProduct);

            if (result > 0)
            {
                return new Response<CreateDealerProductCommand>
                {
                    Success = true,
                    Message = "Dealer Product created successfully.",
                    Data = request
                };
            }

            return new Response<CreateDealerProductCommand>
            {
                Success = false,
                Message = "Failed to create the dealer product.",
                Data = request
            };
        }
    }
}