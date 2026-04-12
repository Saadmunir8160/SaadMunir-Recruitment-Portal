using Application.Common.Interfaces;
using Application.DTOs;
using Application.Queries.Order;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Application.Commands.Order.Create
{
    public class CreateOrderCommand : IRequest<Response<string>>
    {
        [Required(ErrorMessage = "CustomerId is required")]
        public long CustomerId { get; set; }

        [Required(ErrorMessage = "CustomerEmail is required")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "TotalQuantity is required")]
        public long TotalQuantity { get; set; }

        [Required(ErrorMessage = "TotalPrice is required")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "TotalVat is required")]
        public decimal TotalVat { get; set; }

        [Required(ErrorMessage = "ShippingCost is required")]
        public decimal ShippingCost { get; set; }

        //[Required(ErrorMessage = "CouponCode is required")]
        public string? CouponCode { get; set; }

        public string? IpAddress { get; set; }



        public List<OrderItemDTO> orderItemDTOs { get; set; }

        public LocationDTO locationDTO { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Response<string>>
    {
        private readonly ICommandRepository<Domain.Entities.Order> _commandRepository;
        private readonly ICommandRepository<OrderItems> _commandRepositoryOrderItems;
        private readonly ICommandRepository<Location> _commandRepositoryLocation;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IQueryRepository<Domain.Entities.Product> _queryRepository;
        private readonly IQueryRepository<Customer> _queryRepositoryCustomer;
        private readonly IQueryRepository<GeneralSettings> _queryRepositoryGeneral;
        private readonly IMediator _mediator;
        private readonly IEmailService _emailService;
        private readonly IIdentityService _identityService; 
        private readonly IIpLocationService _IpLocationService;
        private readonly ICommandRepository<Domain.Entities.GpsLocation> _commandRepositoryGpsLocation;

        private string customerEmail = string.Empty;
        private long customerID = 0;
        private string SalesEmail = string.Empty;
        private decimal VatPercentage = 0;
        public CreateOrderCommandHandler(ICommandRepository<Domain.Entities.Order> commandRepository,
            IHttpContextAccessor httpContextAccessor,
            ICommandRepository<OrderItems> commandRepositoryOrderItems,
            ICommandRepository<Location> commandRepositoryLocation,
            IQueryRepository<Domain.Entities.Product> queryRepository,
            IMediator mediator,
            IQueryRepository<Customer> queryRepositoryCustomer, 
            IQueryRepository<GeneralSettings> queryRepositoryGeneral,
            IEmailService emailService,
            IIdentityService identityService,
            IIpLocationService IpLocationService,
            ICommandRepository<GpsLocation> commandRepositoryGpsLocation)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _commandRepositoryOrderItems = commandRepositoryOrderItems;
            _commandRepositoryLocation = commandRepositoryLocation;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _queryRepositoryCustomer = queryRepositoryCustomer;
            _queryRepositoryGeneral = queryRepositoryGeneral;
            _emailService = emailService;
            _identityService = identityService;
            _IpLocationService = IpLocationService;
            _commandRepositoryGpsLocation = commandRepositoryGpsLocation;
        }
        
        public async Task<Response<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            string username = GetUsername();
            //if (request.CustomerId != await GetCustomerIDInIDentity())
            //    return ResponseFailure("Wrong UserID, Login with correct Credentials", request);

            var products = await ValidateProducts(request.orderItemDTOs);
            if (products == null || !products.Any())
            {
                return ResponseFailure("Invalid product data.", "Failed");
            }
            var uID = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value ?? string.Empty;
            this.customerID = await GetCustomerIDInIDentity();
            this.customerEmail = await _identityService.GetUserEmailAsync(uID);

            await this.GetGeneralData();

            var (subtotal, shippingCost) = CalculateOrderCosts(request.orderItemDTOs, products);
            decimal grandTotal = subtotal  + shippingCost;

            //Calculate coupon discount
            decimal couponDiscount = 0;
            long? promotionId = null;
            var couponResult = await ValidateAndCalculateCouponDiscount(request.CouponCode, grandTotal, request.locationDTO.CoverageAreaId);
            couponDiscount = couponResult.Item1;
            promotionId = couponResult.Item2; 

            grandTotal -= couponDiscount;

            decimal vat = grandTotal * (this.VatPercentage / 100);

            grandTotal += vat;

            //getting location data from ip
            var locationData = await _IpLocationService.GetLocationAsync(request.IpAddress);

            //Add data to database
            using (var transaction = await _commandRepository.BeginTransactionAsync())
            {
                try
                {
                    long locationId = await HandleLocation(request.locationDTO, username);
                    if (locationId == 0)
                    {
                        return ResponseFailure("Failed to create or update location.", "Faild");
                    }

                    var order = await CreateOrderEntity(request, username, locationId, grandTotal, vat, shippingCost, couponDiscount, promotionId);
                    await _commandRepository.AddAsync(order);

                    var orderItems = CreateOrderItems(request.orderItemDTOs, order.OrderId, username);
                    await _commandRepositoryOrderItems.AddRangeAsync(orderItems);

                    var gpsLocation = await CreateGpsLocationEntity(locationData, order.OrderId);
                    await _commandRepositoryGpsLocation.AddAsync(gpsLocation);

                    await transaction.CommitAsync();


                    bool emailToCustomerSent = true;
                    //bool emailToSalesSent = true;

                    try
                    {
                        await SendEmail(username, this.customerEmail, order.TrackingId);
                    }
                    catch (Exception ex)
                    {
                        emailToCustomerSent = false;
                    }

                    try
                    {
                        await SendEmailSalesTeam(username, this.SalesEmail, order.TrackingId);
                    }
                    catch (Exception ex)
                    {
                        //emailToSalesSent = false;
                    }

                    // Construct a proper response message
                    if (!emailToCustomerSent)
                    {
                        return ResponseSuccess("Order created successfully, but error sending email", order.TrackingId);
                    }



                    return ResponseSuccess("Order created successfully and email sent successfully", order.TrackingId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    throw;
                }
            }

        }

        private string GenerateTrackingId()
        {
            // Example: "ORD-20250116-123456"
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        private string GetUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        private async Task GetGeneralData()
        {
            //var filters = new Dictionary<string, object>
            //  {
            //      { nameof(Domain.Entities.GeneralSettings.SettingsKey), "VATPercentage" }
            //  };
            var result = await _queryRepositoryGeneral.GetAllAsync();

            string vatPercentage = result.FirstOrDefault(r => r.SettingsKey == "VATPercentage")?.SettingsValue ?? "15";
            this.VatPercentage = decimal.TryParse(vatPercentage, out var vat) ? vat : 15m;
            this.SalesEmail = result.FirstOrDefault(r => r.SettingsGroup == "SalesTeam" && r.SettingsKey == "Email")?.SettingsValue ?? "";
                
            //return decimal.TryParse(vatPercentage, out var vat) ? vat : 15m;
        }

        private async Task<long> GetCustomerIDInIDentity()
        {
            var uID = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value ?? string.Empty;


            var filters = new Dictionary<string, object>
            {
                { nameof(Customer.UserId), uID }
            };
            var result = (await _queryRepositoryCustomer.GetByColumnsWithListAsync(filters)).FirstOrDefault();
            return result.CustomerId;

        }

        private async Task<List<Domain.Entities.Product>> ValidateProducts(List<OrderItemDTO> orderItems)
        {
            var filters = new Dictionary<string, object>
        {
            { nameof(Domain.Entities.Product.ProductId), orderItems.Select(i => i.ProductId).ToList() }
        };
            return (await _queryRepository.GetByColumnsWithListAsync(filters)).ToList();
        }

        private (decimal Subtotal, decimal ShippingCost) CalculateOrderCosts(List<OrderItemDTO> orderItems, List<Domain.Entities.Product> products)
        {
            decimal subtotal = 0;
            decimal shippingCost = 0;

            foreach (var item in orderItems)
            {
                var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    var discountedPrice = product.Price - (product.Price * (product.DiscountPercentage / 100));
                    var itemTotal = discountedPrice * item.Quantity;
                    subtotal += itemTotal;

                    var itemShippingCost = itemTotal * (product.ShippingCostPercentage / 100);
                    shippingCost += itemShippingCost;
                }
            }

            return (subtotal, shippingCost);
        }

        private async Task<Tuple<decimal, long?>> ValidateAndCalculateCouponDiscount(string couponCode, decimal subtotal, long coverageAreaId)
        {
            if (string.IsNullOrEmpty(couponCode)) new Tuple<decimal, long?>(0, null);

            var couponValidationQuery = new ValidateCouponQuery { couponCode = couponCode, coverageAreaId = coverageAreaId };
            var couponResult = await _mediator.Send(couponValidationQuery);

            if (couponResult.isValid)
            {
                return new Tuple<decimal, long?>(subtotal * (couponResult.discountPercentage / 100), couponResult.PromotionID);
            }
            else
            {
                return new Tuple<decimal, long?>(0, null);
            }

            //return couponResult.isValid ? subtotal * (couponResult.discountPercentage / 100) : 0;
        }

        private async Task<Domain.Entities.Order> CreateOrderEntity(CreateOrderCommand request, string username, long locationId, decimal grandTotal, decimal vat, decimal shippingCost, decimal couponDiscount, long? promotionId)
        {
            return new Domain.Entities.Order
            {
                CustomerId = this.customerID,
                LocationId = locationId,
                TrackingId = GenerateTrackingId(),
                TotalQuantity = request.TotalQuantity,
                TotalPrice = grandTotal,
                TotalVat = vat,
                ShipingCost = shippingCost,
                CouponDiscount = couponDiscount,
                PromotionId = promotionId,
                CreatedDate = DateTime.Now,
                CreatedBy = string.IsNullOrEmpty(username) ? null : username,
            };
        }

        private async Task<Domain.Entities.GpsLocation> CreateGpsLocationEntity(IpLocationDTO data, long orderId)
        {
            return new Domain.Entities.GpsLocation
            {
                OrderId = orderId,
                IpAddress = data.Query,
                City = data.City,
                Country = data.Country,
                RegionName = data.RegionName,
                Longitude = data.Longitude,
                Latitude = data.Latitude,
            };
        }

        private List<OrderItems> CreateOrderItems(List<OrderItemDTO> orderItems, long orderId, string username)
        {
            return orderItems.Select(item => new OrderItems
            {
                OrderId = orderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                NumberOfTrucks = item.NumberOfTrucks,
                CreatedDate = DateTime.Now,
                CreatedBy = string.IsNullOrEmpty(username) ? null : username,
            }).ToList();
        }


        private async Task<long> HandleLocation(LocationDTO locationDTO, string username)
        {
            if (locationDTO.LocationID > 0)
            {
                return locationDTO.LocationID;
            }

            var location = new Domain.Entities.Location
            {
                CustomerId = this.customerID,
                CoverageAreaId = locationDTO.CoverageAreaId,
                CitiesId = locationDTO.CityId,
                Address = locationDTO.Address,
                ZipCode = locationDTO.ZipCode,
                GpsCoordinates = locationDTO.GpsCoordinates,
                CreatedBy = string.IsNullOrEmpty(username) ? null : username,
                CreatedDate = DateTime.Now
            };

            await _commandRepositoryLocation.AddAsync(location);

            return location.LocationId;
        }

        private async Task SendEmail(string customerName, string CustomerEmail, string TrackingId)
        {
            var subject = $"Order Confirmation - Order #{TrackingId}";
            var body = $@"
                    <h2>Thank You for Your Order, {customerName}!</h2>
                    <p>We're excited to let you know that your order has been successfully placed.</p>
                    <ul>
                        <li><strong>Tracking ID:</strong> {TrackingId}</li>
                    </ul>
                    <p>You can use the tracking ID to follow your order's journey.</p>
                    <p>If you have any questions, feel free to reach out to our support team.</p>
                    <p>Thank you for shopping with us!</p>
                    <p><strong>Best regards,</strong><br>The United Cement Industrial Company Team</p>";

            await _emailService.SendEmailAsync(CustomerEmail, subject, body);
        }


        private async Task SendEmailSalesTeam(string customerName, string CustomerEmail, string TrackingId)
        {
            var subject = $"Order Confirmation - Order #{TrackingId}";
            var body = $@"
                    <h2>New Order Placed for {customerName}</h2>
                    <p>A new order has been placed with the following details:</p>
                    <ul>
                        <li><strong>Tracking ID:</strong> {TrackingId}</li>
                    </ul>
                    <p>Please use the tracking ID to monitor the order's progress.</p>
                    <p><strong>Best regards,</strong><br>The United Cement Industrial Company Team</p>";

            await _emailService.SendEmailAsync(CustomerEmail, subject, body);
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
