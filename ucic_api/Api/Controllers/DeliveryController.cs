using Application.Commands.Delivery.Update;
using Application.Queries.Delivery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DeliveryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get delivery information by LnOrderNumber. This endpoint is used by the frontend Angular app.
        /// </summary>
        /// <param name="lnOrderNumber">The LN Order Number to search for</param>
        /// <returns>Delivery information</returns>
        [HttpGet("{lnOrderNumber}")]
        [Authorize(Roles = "Admin,Dealer")]
        public async Task<ActionResult> GetByLnOrderNumber([FromRoute] string lnOrderNumber)
        {
            if (string.IsNullOrWhiteSpace(lnOrderNumber))
            {
                return BadRequest(new { Success = false, Message = "LnOrderNumber is required." });
            }

            try
            {
                var query = new GetDeliveryByLnOrderNumberQuery
                {
                    LnOrderNumber = lnOrderNumber
                };

                var result = await _mediator.Send(query);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving delivery information.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Update delivery information. Requires API key authentication via X-API-Key header.
        /// This endpoint receives delivery data from external API calls.
        /// </summary>
        /// <param name="request">Request body containing delivery information</param>
        /// <returns>Success response</returns>
        [HttpPost("Update")]
        [ApiKey]
        public async Task<ActionResult> Update([FromBody] DeliveryUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new UpdateDeliveryCommand
            {
                LnOrderNumber = request.LnOrderNumber,
                CustomerOrder = request.CustomerOrder,
                IQN = request.IQN,
                InternalSalesRepresentative = request.InternalSalesRepresentative,
                QuantityShipped = request.QuantityShipped,
                ItemDescription = request.ItemDescription,
                DateOut = request.DateOut,
                DateIN = request.DateIN,
                WeightIN = request.WeightIN,
                WeightOut = request.WeightOut,
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
                AreaDescription = request.AreaDescription
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { Success = true, Message = "Delivery information saved successfully.", DeliveryID = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while saving delivery information.", Error = ex.Message });
            }
        }
    }

    public class DeliveryUpdateRequest
    {
        [MaxLength(50)]
        [System.Text.Json.Serialization.JsonPropertyName("Order")]
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
}

