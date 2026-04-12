using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands.Delivery.Update
{
    public class UpdateDeliveryCommand : IRequest<int>
    {
        [MaxLength(50)]
        public string? LnOrderNumber { get; set; }

        [MaxLength(50)]
        public string? CustomerOrder { get; set; }

        [MaxLength(50)]
        public string? IQN { get; set; }

        [MaxLength(50)]
        public string? InternalSalesRepresentative { get; set; }

        public string? QuantityShipped { get; set; }

        [MaxLength(500)]
        public string? ItemDescription { get; set; }

        public string? DateOut { get; set; }

        public string? DateIN { get; set; }

        public string? WeightIN { get; set; }

        public string? WeightOut { get; set; }

        [MaxLength(50)]
        public string? ProductionOrder { get; set; }

        [MaxLength(50)]
        public string? Item { get; set; }

        [MaxLength(50)]
        public string? Line { get; set; }

        [MaxLength(50)]
        public string? Shipment { get; set; }

        [MaxLength(50)]
        public string? ShipmentLine { get; set; }

        [MaxLength(200)]
        public string? WarehouseDescription { get; set; }

        [MaxLength(200)]
        public string? DriverName { get; set; }

        [MaxLength(100)]
        public string? Car { get; set; }

        [MaxLength(100)]
        public string? DeliveryMeans { get; set; }

        [MaxLength(500)]
        public string? CustomerName { get; set; }

        [MaxLength(200)]
        public string? TransporterName { get; set; }

        [MaxLength(50)]
        public string? Area { get; set; }

        [MaxLength(200)]
        public string? AreaDescription { get; set; }
    }

    public class UpdateDeliveryCommandHandler : IRequestHandler<UpdateDeliveryCommand, int>
    {
        private readonly ICommandRepository<Domain.Entities.Delivery> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Delivery> _queryRepository;

        public UpdateDeliveryCommandHandler(
            ICommandRepository<Domain.Entities.Delivery> commandRepository,
            IQueryRepository<Domain.Entities.Delivery> queryRepository)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
        }

        public async Task<int> Handle(UpdateDeliveryCommand request, CancellationToken cancellationToken)
        {
            // Check if delivery already exists with the same LnOrderNumber
            Domain.Entities.Delivery? existingDelivery = null;
            
            if (!string.IsNullOrWhiteSpace(request.LnOrderNumber))
            {
                existingDelivery = await _queryRepository.GetQueryable()
                    .FirstOrDefaultAsync(x => x.LnOrderNumber == request.LnOrderNumber 
                        && !x.IsDeleted 
                        && x.IsActive, cancellationToken);
            }

            if (existingDelivery != null)
            {
                // Update existing record
                existingDelivery.CustomerOrder = request.CustomerOrder;
                existingDelivery.IQN = request.IQN;
                existingDelivery.InternalSalesRepresentative = request.InternalSalesRepresentative;
                existingDelivery.QuantityShipped = ParseDecimal(request.QuantityShipped);
                existingDelivery.ItemDescription = request.ItemDescription;
                existingDelivery.DateOut = ParseDateTime(request.DateOut);
                existingDelivery.DateIN = ParseDateTime(request.DateIN);
                existingDelivery.WeightIN = ParseDecimal(request.WeightIN);
                existingDelivery.WeightOut = ParseDecimal(request.WeightOut);
                existingDelivery.ProductionOrder = request.ProductionOrder;
                existingDelivery.Item = request.Item;
                existingDelivery.Line = request.Line;
                existingDelivery.Shipment = request.Shipment;
                existingDelivery.ShipmentLine = request.ShipmentLine;
                existingDelivery.WarehouseDescription = request.WarehouseDescription;
                existingDelivery.DriverName = request.DriverName;
                existingDelivery.Car = request.Car;
                existingDelivery.DeliveryMeans = request.DeliveryMeans;
                existingDelivery.CustomerName = request.CustomerName;
                existingDelivery.TransporterName = request.TransporterName;
                existingDelivery.Area = request.Area;
                existingDelivery.AreaDescription = request.AreaDescription;
                existingDelivery.ModifiedDate = DateTime.UtcNow;

                await _commandRepository.UpdateAsync(existingDelivery);
                return existingDelivery.DeliveryID;
            }
            else
            {
                // Create new record
                var delivery = new Domain.Entities.Delivery
                {
                    LnOrderNumber = request.LnOrderNumber,
                    CustomerOrder = request.CustomerOrder,
                    IQN = request.IQN,
                    InternalSalesRepresentative = request.InternalSalesRepresentative,
                    QuantityShipped = ParseDecimal(request.QuantityShipped),
                    ItemDescription = request.ItemDescription,
                    DateOut = ParseDateTime(request.DateOut),
                    DateIN = ParseDateTime(request.DateIN),
                    WeightIN = ParseDecimal(request.WeightIN),
                    WeightOut = ParseDecimal(request.WeightOut),
                    ProductionOrder = request.ProductionOrder,
                    Item = request.Item,
                    Line = request.Line,
                    Shipment = request.Shipment,
                    ShipmentLine = request.ShipmentLine,
                    WarehouseDescription = request.WarehouseDescription,
                    DriverName = request.DriverName,
                    Car = request.Car,
                    DeliveryMeans = request.DeliveryMeans,
                    CustomerName = request.CustomerName,
                    TransporterName = request.TransporterName,
                    Area = request.Area,
                    AreaDescription = request.AreaDescription,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _commandRepository.AddAsync(delivery);
                return delivery.DeliveryID;
            }
        }

        private static decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private static DateTime? ParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Try parsing with common formats
            string[] formats = {
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy"
            };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            // Fallback to standard parsing
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            return null;
        }
    }
}

