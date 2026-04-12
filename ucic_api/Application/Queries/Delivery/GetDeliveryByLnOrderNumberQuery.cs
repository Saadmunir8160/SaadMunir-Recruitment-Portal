using Application.DTOs;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries.Delivery
{
    public class GetDeliveryByLnOrderNumberQuery : IRequest<Response<DeliveryDTO>>
    {
        public string LnOrderNumber { get; set; } = string.Empty;
    }

    public class GetDeliveryByLnOrderNumberQueryHandler : IRequestHandler<GetDeliveryByLnOrderNumberQuery, Response<DeliveryDTO>>
    {
        private readonly IQueryRepository<Domain.Entities.Delivery> _queryRepository;

        public GetDeliveryByLnOrderNumberQueryHandler(
            IQueryRepository<Domain.Entities.Delivery> queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<Response<DeliveryDTO>> Handle(GetDeliveryByLnOrderNumberQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var delivery = await _queryRepository.GetQueryable()
                    .FirstOrDefaultAsync(x => x.LnOrderNumber == request.LnOrderNumber 
                        && !x.IsDeleted 
                        && x.IsActive, cancellationToken);

                if (delivery == null)
                {
                    return new Response<DeliveryDTO>
                    {
                        Success = false,
                        Message = $"Delivery with LnOrderNumber '{request.LnOrderNumber}' not found"
                    };
                }

                var deliveryDTO = new DeliveryDTO
                {
                    DeliveryID = delivery.DeliveryID,
                    LnOrderNumber = delivery.LnOrderNumber,
                    CustomerOrder = delivery.CustomerOrder,
                    IQN = delivery.IQN,
                    InternalSalesRepresentative = delivery.InternalSalesRepresentative,
                    QuantityShipped = delivery.QuantityShipped,
                    ItemDescription = delivery.ItemDescription,
                    DateOut = delivery.DateOut,
                    DateIN = delivery.DateIN,
                    WeightIN = delivery.WeightIN,
                    WeightOut = delivery.WeightOut,
                    ProductionOrder = delivery.ProductionOrder,
                    Item = delivery.Item,
                    Line = delivery.Line,
                    Shipment = delivery.Shipment,
                    ShipmentLine = delivery.ShipmentLine,
                    WarehouseDescription = delivery.WarehouseDescription,
                    DriverName = delivery.DriverName,
                    Car = delivery.Car,
                    DeliveryMeans = delivery.DeliveryMeans,
                    CustomerName = delivery.CustomerName,
                    TransporterName = delivery.TransporterName,
                    Area = delivery.Area,
                    AreaDescription = delivery.AreaDescription,
                    IsActive = delivery.IsActive,
                    IsDeleted = delivery.IsDeleted,
                    CreatedDate = delivery.CreatedDate,
                    CreatedBy = delivery.CreatedBy,
                    ModifiedDate = delivery.ModifiedDate,
                    ModifiedBy = delivery.ModifiedBy
                };

                return new Response<DeliveryDTO>
                {
                    Success = true,
                    Message = "Delivery retrieved successfully",
                    Data = deliveryDTO
                };
            }
            catch (Exception ex)
            {
                return new Response<DeliveryDTO>
                {
                    Success = false,
                    Message = $"Error retrieving delivery: {ex.Message}"
                };
            }
        }
    }
}

