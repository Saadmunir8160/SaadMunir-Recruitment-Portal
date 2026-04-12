using Application.Common.Interfaces;
using Application.DTOs;
using Application.DTOs.DealerOrder;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.Common.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;

namespace Application.Commands.DealerOrder.Create
{
    public class CreateDealerOrderCommand : IRequest<Response<string>>
    {
        // Remove DealerID from request - will be set from authenticated user
        public int? DriverID { get; set; }
        public int? VehicleID { get; set; }
        public int? AddressID { get; set; }
        // New: AreaID to store selected delivery area
        public int? AreaID { get; set; }
        [Required(ErrorMessage = "Customer order number is required")]
        public string? CustomerOrderNumber { get; set; }
        public string? PortalOrderNumber { get; set; }
        public string? TransporterName { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? Status { get; set; }
        public decimal? TotalAmount { get; set; }
        [Required(ErrorMessage = "Order items are required")]
        public List<Application.DTOs.DealerOrder.CreateDealerOrderItemDTO> OrderItems { get; set; } = new List<Application.DTOs.DealerOrder.CreateDealerOrderItemDTO>();
    }

    public class CreateDealerOrderCommandHandler : IRequestHandler<CreateDealerOrderCommand, Response<string>>
    {
        private readonly Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrder> _dealerOrderRepository;
        private readonly Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrderItem> _dealerOrderItemRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerProduct> _dealerProductRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> _dealerOrderQueryRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerArea> _dealerAreaRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerDriver> _dealerDriverRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerVehicle> _dealerVehicleRepository;
        private readonly Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IIdentityService _identityService;
        private readonly IExternalApiService _externalApiService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateDealerOrderCommandHandler> _logger;

        public CreateDealerOrderCommandHandler(
            Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrder> dealerOrderRepository,
            Domain.Repositories.Command.Base.ICommandRepository<Domain.Entities.DealerOrderItem> dealerOrderItemRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerProduct> dealerProductRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerOrder> dealerOrderQueryRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerArea> dealerAreaRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerDriver> dealerDriverRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.DealerVehicle> dealerVehicleRepository,
            Domain.Repositories.Query.Base.IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IIdentityService identityService,
            IExternalApiService externalApiService,
            IEmailService emailService,
            ILogger<CreateDealerOrderCommandHandler> logger)
        {
            _dealerOrderRepository = dealerOrderRepository;
            _dealerOrderItemRepository = dealerOrderItemRepository;
            _dealerProductRepository = dealerProductRepository;
            _dealerOrderQueryRepository = dealerOrderQueryRepository;
            _dealerAreaRepository = dealerAreaRepository;
            _dealerDriverRepository = dealerDriverRepository;
            _dealerVehicleRepository = dealerVehicleRepository;
            _dealerRepository = dealerRepository;
            _identityService = identityService;
            _externalApiService = externalApiService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(CreateDealerOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get current dealer ID using centralized method
                var currentDealerId = await _identityService.GetCurrentDealerIdAsync();
                if (currentDealerId == null)
                {
                    return new Response<string>
                    {
                        Success = false,
                        Message = "Dealer not found for current user"
                    };
                }

                //// Validate that all products exist and belong to current dealer
                //foreach (var item in request.OrderItems)
                //{
                //    var product = await _dealerProductRepository.GetByIdAsync(item.DealerProductID);
                //    if (product == null)
                //    {
                //        return new Response<string>
                //        {
                //            Success = false,
                //            Message = $"Product with ID {item.DealerProductID} not found"
                //        };
                //    }

                //    // Ensure product belongs to current dealer for security
                //    if (product.DealerID != currentDealerId.Value)
                //    {
                //        return new Response<string>
                //        {
                //            Success = false,
                //            Message = $"Product with ID {item.DealerProductID} does not belong to current dealer"
                //        };
                //    }
                //}

                // Create the dealer order
                    // Validate customer order number presence (defensive)
                    if (string.IsNullOrWhiteSpace(request.CustomerOrderNumber))
                    {
                        return new Response<string>
                        {
                            Success = false,
                            Message = "Customer order number is required"
                        };
                    }

                    // Check uniqueness of CustomerOrderNumber
                    var exists = await _dealerOrderQueryRepository.ValueExistsAsync("CustomerOrderNumber", request.CustomerOrderNumber);
                    if (exists)
                    {
                        return new Response<string>
                        {
                            Success = false,
                            Message = "Customer order number already exists"
                        };
                    }

                    // Fetch required data for external API payload
                    string? areaCode = null;
                    if (request.AreaID.HasValue)
                    {
                        var dealerArea = await _dealerAreaRepository.GetByIdAsync(request.AreaID.Value);
                        areaCode = dealerArea?.AreaCode;
                    }

                    string? iqamaNumber = null;
                    if (request.DriverID.HasValue)
                    {
                        var dealerDriver = await _dealerDriverRepository.GetByIdAsync(request.DriverID.Value);
                        iqamaNumber = dealerDriver?.IqamaNumber;
                    }

                    string? vehicleLnId = null;
                    if (request.VehicleID.HasValue)
                    {
                        var dealerVehicle = await _dealerVehicleRepository.GetByIdAsync(request.VehicleID.Value);
                        vehicleLnId = dealerVehicle?.Ln_ID;
                    }

                    string? dealerLnId = null;
                    var dealer = await _dealerRepository.GetByIdAsync(currentDealerId.Value);
                    dealerLnId = dealer?.Ln_ID;

                    // Create the dealer order
                var dealerOrder = new Domain.Entities.DealerOrder
                {
                    DealerID = currentDealerId.Value, // Use centralized dealer ID
                    DriverID = request.DriverID,
                    VehicleID = request.VehicleID,
                    AddressID = request.AddressID,
                    AreaID = request.AreaID,
                    CustomerOrderNumber = request.CustomerOrderNumber,
                    PortalOrderNumber = request.PortalOrderNumber,
                    TransporterName = request.TransporterName,
                    OrderDate = request.OrderDate,
                    Status = request.Status,
                    TotalAmount = request.TotalAmount,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _identityService.GetCurrentUserId()
                };

                await _dealerOrderRepository.AddAsync(dealerOrder);

                // Create order items
                foreach (var item in request.OrderItems)
                {
                    // Fetch Product_LnCode from DealerProducts table
                    string? productLnCode = null;
                    if (item.DealerProductID > 0)
                    {
                        var dealerProduct = await _dealerProductRepository.GetByIdAsync(item.DealerProductID);
                        productLnCode = dealerProduct?.Product_LnCode;
                    }
                    
                    // Use Product_LnCode from DealerProducts if available, otherwise fallback to item.Product_LnCode
                    productLnCode = productLnCode ?? item.Product_LnCode;

                    var orderItem = new Domain.Entities.DealerOrderItem
                    {
                        DealerOrderID = dealerOrder.DealerOrderID,
                        DealerProductID = item.DealerProductID,
                        Product_LnCode = item.Product_LnCode,
                        ProductDescription = item.ProductDescription,
                        Quantity = item.Quantity,
                        Unit = item.Unit, // Set the unit from the DTO
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = _identityService.GetCurrentUserId()
                    };

                    await _dealerOrderItemRepository.AddAsync(orderItem);



                    //=============================================
                    // Build payload for external API
                    var payload = new
                    {
                        data = new[]
                        {
                     new
                     {
                         External_Order = request.CustomerOrderNumber,
                         External_Position = "1",
                         External_Customer = "NA",
                         Item = productLnCode ?? item.Product_LnCode,
                         Qty_Ordered = item.Quantity.ToString() ?? "0",
                         Unit = item.Unit ?? "BAG",
                         Price = request.TotalAmount?.ToString() ?? "0",
                         Line_Discount = string.Empty,
                         Total_Amount = "",
                         Currency = "SAR",
                         ReferenceA = "REF1",
                         ReferenceB = "REF2",
                         Customer_Order_Number = request.CustomerOrderNumber ?? string.Empty,
                         Business_Partner = dealerLnId,
                         Car = vehicleLnId,
                         Transport_Name = request.TransporterName ?? string.Empty,
                         Transport_Mode = "ECU",
                         Iqama = iqamaNumber,
                         Area = areaCode,
                         Portal_Order = request.PortalOrderNumber ?? string.Empty
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
                                        dealerOrder.Ln_OrderNumber = orderLnNumber;
                                        dealerOrder.ModifiedDate = DateTime.UtcNow;
                                        dealerOrder.ModifiedBy = _identityService.GetCurrentUserId();
                                        await _dealerOrderRepository.UpdateAsync(dealerOrder);
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
                        // Log the exception but don't fail the order creation process entirely
                        _logger.LogError(ex, "Failed to create sales order in external system: {Message}", ex.Message);
                        await SendFailureNotificationToSalesTeamAsync(ex, request.CustomerOrderNumber ?? "N/A");
                    }
                    //=============================================
                }

                return new Response<string>
                {
                    Success = true,
                    Message = "Dealer order created successfully",
                    Data = dealerOrder.DealerOrderID.ToString()
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Success = false,
                    Message = $"Error creating dealer order: {ex.Message}"
                };
            }
        }

        private async Task SendFailureNotificationToSalesTeamAsync(Exception ex, string customerOrderNumber)
        {
            try
            {
                var salesUsers = await _identityService.GetUsersByRoleAsync("Sale");
                if (salesUsers.Count == 0)
                {
                    _logger.LogWarning("No users with Sales role found to notify about external API failure");
                    return;
                }

                var subject = $"Alert: Failed to create sales order in external system - Order {customerOrderNumber}";
                var body = $@"
                    <h2>External API Failure Alert</h2>
                    <p>The system failed to create a sales order in the external system.</p>
                    <ul>
                        <li><strong>Customer Order Number:</strong> {customerOrderNumber}</li>
                        <li><strong>Error:</strong> {System.Net.WebUtility.HtmlEncode(ex.Message)}</li>
                        <li><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                    </ul>
                    <p>The dealer order was created locally but may need to be synchronized with the external system manually.</p>
                    <p><strong>Best regards,</strong><br>The United Cement Industrial Company Team</p>";

                foreach (var (_, fullName, _, email, _, _) in salesUsers)
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(email, subject, body);
                            _logger.LogInformation("Sent failure notification to Sales user: {Email}", email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Failed to send failure notification email to {Email}", email);
                        }
                    }
                }
            }
            catch (Exception notificationEx)
            {
                _logger.LogError(notificationEx, "Failed to send failure notifications to Sales team");
            }
        }
    }
}