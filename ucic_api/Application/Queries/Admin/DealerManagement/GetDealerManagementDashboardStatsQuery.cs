using Application.DTOs;
using Application.Queries.Admin.DealerProduct;
using Application.Queries.Admin.DealerOrder;
using Application.Queries.Admin.DealerDriver;
using Application.Queries.Admin.DealerVehicle;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Admin.DealerManagement
{
    public class GetDealerManagementDashboardStatsQuery : IRequest<DealerManagementDashboardStatsDTO>
    {
    }

    public class GetDealerManagementDashboardStatsQueryHandler : IRequestHandler<GetDealerManagementDashboardStatsQuery, DealerManagementDashboardStatsDTO>
    {
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IMediator _mediator;

        public GetDealerManagementDashboardStatsQueryHandler(
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IMediator mediator)
        {
            _dealerRepository = dealerRepository;
            _mediator = mediator;
        }

        public async Task<DealerManagementDashboardStatsDTO> Handle(GetDealerManagementDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var dealers = await _dealerRepository.GetAllAsync();
            
            // Get stats from individual services
            var productStats = await _mediator.Send(new GetDealerProductsStatsForAdminQuery(), cancellationToken);
            var orderStats = await _mediator.Send(new GetDealerOrdersStatsForAdminQuery(), cancellationToken);
            var driverStats = await _mediator.Send(new GetDealerDriversStatsForAdminQuery(), cancellationToken);
            var vehicleStats = await _mediator.Send(new GetDealerVehiclesStatsForAdminQuery(), cancellationToken);

            return new DealerManagementDashboardStatsDTO
            {
                TotalDealers = dealers.Count(),
                ActiveDealers = dealers.Count(d => d.IsActive),
                ProductStats = productStats,
                OrderStats = orderStats,
                DriverStats = driverStats,
                VehicleStats = vehicleStats
            };
        }
    }

    public class GetDealersSummaryForAdminQuery : IRequest<List<DealerSummaryForAdminDTO>>
    {
    }

    public class GetDealersSummaryForAdminQueryHandler : IRequestHandler<GetDealersSummaryForAdminQuery, List<DealerSummaryForAdminDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Dealer> _dealerRepository;
        private readonly IQueryRepository<Domain.Entities.DealerProduct> _productRepository;
        private readonly IQueryRepository<Domain.Entities.DealerOrder> _orderRepository;
        private readonly IQueryRepository<Domain.Entities.DealerDriver> _driverRepository;
        private readonly IQueryRepository<Domain.Entities.DealerVehicle> _vehicleRepository;

        public GetDealersSummaryForAdminQueryHandler(
            IQueryRepository<Domain.Entities.Dealer> dealerRepository,
            IQueryRepository<Domain.Entities.DealerProduct> productRepository,
            IQueryRepository<Domain.Entities.DealerOrder> orderRepository,
            IQueryRepository<Domain.Entities.DealerDriver> driverRepository,
            IQueryRepository<Domain.Entities.DealerVehicle> vehicleRepository)
        {
            _dealerRepository = dealerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<List<DealerSummaryForAdminDTO>> Handle(GetDealersSummaryForAdminQuery request, CancellationToken cancellationToken)
        {
            var dealers = await _dealerRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            var drivers = await _driverRepository.GetAllAsync();
            var vehicles = await _vehicleRepository.GetAllAsync();

            var dealerSummaries = dealers.Select(dealer => new DealerSummaryForAdminDTO
            {
                DealerId = dealer.DealerId,
                Name = dealer.DealerName,
                IsActive = dealer.IsActive,
                TotalProducts = products.Count(p => p.DealerID == dealer.DealerId),
                TotalOrders = orders.Count(o => o.DealerID == dealer.DealerId),
                TotalDrivers = drivers.Count(d => d.DealerID == dealer.DealerId),
                TotalVehicles = vehicles.Count(v => v.DealerID == dealer.DealerId),
                TotalOrderValue = orders.Where(o => o.DealerID == dealer.DealerId && o.TotalAmount.HasValue)
                                       .Sum(o => o.TotalAmount!.Value),
                CreatedDate = dealer.CreatedDate,
                LastOrderDate = orders.Where(o => o.DealerID == dealer.DealerId)
                                     .OrderByDescending(o => o.OrderDate)
                                     .FirstOrDefault()?.OrderDate
            }).ToList();

            return dealerSummaries.OrderByDescending(d => d.TotalOrderValue).ToList();
        }
    }
}