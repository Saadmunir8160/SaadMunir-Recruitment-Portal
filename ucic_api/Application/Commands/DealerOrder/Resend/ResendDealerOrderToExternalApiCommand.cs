using Application.Common.Interfaces;
using Application.Common.Services;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.DealerOrder.Resend
{
    public class ResendDealerOrderToExternalApiCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "Customer order number is required")]
        public string CustomerOrderNumber { get; set; } = string.Empty;
    }

    public class ResendDealerOrderToExternalApiCommandHandler : IRequestHandler<ResendDealerOrderToExternalApiCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderQueryRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrderItem> _dealerOrderItemQueryRepository;
        private readonly IQueryRepository<Domain.Entities.DealerArea> _dealerAreaRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _dealerDriverRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _dealerVehicleRepository;
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _dealerProductRepository;
        private readonly IIdentityService _identityService;
        private readonly IExternalApiService _externalApiService;
        private readonly ILogger<ResendDealerOrderToExternalApiCommandHandler> _logger;

        public ResendDealerOrderToExternalApiCommandHandler(
            ICommandRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            IQueryRepository<Domain.Entities.DealerOrder> dealerOrderQueryRepository,
            IQueryRepository<Domain.Entities.DealerOrderItem> dealerOrderItemQueryRepository,
            IQueryRepository<Domain.Entities.DealerArea> dealerAreaRepository,
            IQueryRepository<Domain.Entities.DealerDriver> dealerDriverRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> dealerVehicleRepository,
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IQueryRepository<Domain.Entities.DealerProduct> dealerProductRepository,
            IIdentityService identityService,
            IExternalApiService externalApiService,
            ILogger<ResendDealerOrderToExternalApiCommandHandler> logger)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _dealerOrderQueryRepository = dealerOrderQueryRepository;
            _dealerOrderItemQueryRepository = dealerOrderItemQueryRepository;
            _dealerAreaRepository = dealerAreaRepository;
            _dealerDriverRepository = dealerDriverRepository;
            _dealerVehicleRepository = dealerVehicleRepository;
            _dealerRepository = dealerRepository;
            _dealerProductRepository = dealerProductRepository;
            _identityService = identityService;
            _externalApiService = externalApiService;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(ResendDealerOrderToExternalApiCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerOrderNumber))
            {
                return new Response<string>
                {
                    Success = false,
                    Message = "Customer order number is required"
                };
            }

            var order = await _dealerOrderQueryRepository.GetQueryable()
                .FirstOrDefaultAsync(o => o.CustomerOrderNumber == request.CustomerOrderNumber && !o.IsDeleted && o.IsActive, cancellationToken);

            if (order == null)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = $"Dealer order with customer order number '{request.CustomerOrderNumber}' not found"
                };
            }

            // Optional: restrict Dealer role to their own orders only (Admin can resend any)
            var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
            if (currentDealerId.HasValue && order.DealerID != currentDealerId.Value)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = "You do not have permission to resend this order"
                };
            }

            var orderItems = await _dealerOrderItemQueryRepository.GetQueryable()
                .Where(oi => oi.DealerOrderID == order.DealerOrderID && !oi.IsDeleted && oi.IsActive)
                .ToListAsync(cancellationToken);

            if (orderItems.Count == 0)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = "No order items found for this order"
                };
            }

            // Resolve codes for payload (same as CreateDealerOrderCommand)
            string? areaCode = null;
            if (order.AreaID.HasValue)
            {
                var dealerArea = await _dealerAreaRepository.GetByIdAsync(order.AreaID.Value);
                areaCode = dealerArea?.AreaCode;
            }

            string? iqamaNumber = null;
            if (order.DriverID.HasValue)
            {
                var dealerDriver = await _dealerDriverRepository.GetByIdAsync(order.DriverID.Value);
                iqamaNumber = dealerDriver?.IqamaNumber;
            }

            string? vehicleLnId = null;
            if (order.VehicleID.HasValue)
            {
                var dealerVehicle = await _dealerVehicleRepository.GetByIdAsync(order.VehicleID.Value);
                vehicleLnId = dealerVehicle?.Ln_ID;
            }

            var dealer = await _dealerRepository.GetByIdAsync(order.DealerID);
            var dealerLnId = dealer?.Ln_ID;

            string? lastLnOrderNumber = null;
            var position = 1;

            foreach (var item in orderItems)
            {
                string? productLnCode = item.Product_LnCode;
                if (item.DealerProductID > 0)
                {
                    var dealerProduct = await _dealerProductRepository.GetByIdAsync(item.DealerProductID);
                    productLnCode = dealerProduct?.Product_LnCode ?? item.Product_LnCode;
                }

                //=============================================
                // Build payload for external API
                var payload = new
                {
                    data = new[]
                    {
                     new
                     {
                         External_Order = order.CustomerOrderNumber,
                         External_Position = "1",
                         External_Customer = "NA",
                         Item = productLnCode ?? item.Product_LnCode,
                         Qty_Ordered = item.Quantity.ToString() ?? "0",
                         Unit = item.Unit ?? "BAG",
                         Price = order.TotalAmount?.ToString() ?? "0",
                         Line_Discount = string.Empty,
                         Total_Amount = "",
                         Currency = "SAR",
                         ReferenceA = "REF1",
                         ReferenceB = "REF2",
                         Customer_Order_Number = order.CustomerOrderNumber ?? string.Empty,
                         Business_Partner = dealerLnId,
                         Car = vehicleLnId,
                         Transport_Name = order.TransporterName ?? string.Empty,
                         Transport_Mode = "ECU",
                         Iqama = iqamaNumber,
                         Area = areaCode,
                         Portal_Order = order.PortalOrderNumber ?? string.Empty
                     }
                 }
                };

                // Call external API
                try
                {
                    var result = await _externalApiService.PostAsync<dynamic>("CreateSalesOrder", payload);

                    // Update dealer order with response data
                    var jsonElement = (System.Text.Json.JsonElement)result;

                    if (jsonElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        // Extract the order Ln number from the response
                        if (jsonElement.TryGetProperty("result", out var resultProperty) &&
                            resultProperty.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var resultValue = resultProperty.GetString();
                            if (!string.IsNullOrEmpty(resultValue))
                            {
                                var parts = resultValue.Split('|');
                                if (parts.Length > 1)
                                {
                                    var orderLnNumber = parts[1];
                                    // Update the dealer order with the Ln_OrderNumber
                                    order.Ln_OrderNumber = orderLnNumber;
                                    order.ModifiedDate = DateTime.UtcNow;
                                    order.ModifiedBy = _identityService.GetCurrentUserId();
                                    await _dealerOrderRepository.UpdateAsync(order);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to get valid response from external API");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resend order item {OrderItemID} to external API for order {CustomerOrderNumber}", item.OrderItemID, request.CustomerOrderNumber);
                    return new Response<string>
                    {
                        Success = false,
                        Message = $"Failed to resend to external system: {ex.Message}"
                    };
                }

                position++;
            }

            if (!string.IsNullOrEmpty(lastLnOrderNumber))
            {
                order.Ln_OrderNumber = lastLnOrderNumber;
                order.ModifiedDate = DateTime.UtcNow;
                order.ModifiedBy = _identityService.GetCurrentUserId();
                await _dealerOrderRepository.UpdateAsync(order);
            }

            return new Response<string>
            {
                Success = true,
                Message = "Order resent to external system successfully",
                Data = lastLnOrderNumber ?? order.Ln_OrderNumber ?? string.Empty
            };
        }
    }
}
