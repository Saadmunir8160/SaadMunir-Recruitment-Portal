using Application.Commands.Order.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStatusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderStatusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Update the status of a dealer order by Ln_OrderNumber. Requires API key authentication via X-API-Key header.
        /// </summary>
        /// <param name="request">Request body containing Ln_OrderNumber and Status</param>
        /// <returns>Success response</returns>
        [HttpPut("Update")]
        [ApiKey]
        public async Task<ActionResult> UpdateOrderStatus([FromBody] OrderStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Ln_OrderNumber))
            {
                return BadRequest(new { Success = false, Message = "Ln_OrderNumber is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { Success = false, Message = "Status is required." });
            }

            if (request.Status.Length > 50)
            {
                return BadRequest(new { Success = false, Message = "Status cannot exceed 50 characters." });
            }

            var command = new UpdateOrderStatusCommand
            {
                Ln_OrderNumber = request.Ln_OrderNumber,
                Status = request.Status,
                AvailableCredit = request.AvailableCredit,
                CarNumber = request.CarNumber,
                TransporterName = request.TransporterName,
                TransporterCode = request.TransporterCode,
                IqamaNumber = request.IqamaNumber,
                DriverName = request.DriverName,
                Area = request.Area
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { Success = true, Message = "Dealer order status updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while updating the order status.", Error = ex.Message });
            }
        }
    }

    public class OrderStatusRequest
    {
        [Required(ErrorMessage = "Ln_OrderNumber is required")]
        [System.Text.Json.Serialization.JsonPropertyName("ln_OrderNumber")]
        public string Ln_OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = string.Empty;

        [MaxLength(50)]
        [System.Text.Json.Serialization.JsonPropertyName("availableCredit")]
        public string? AvailableCredit { get; set; }

        [MaxLength(50)]
        [System.Text.Json.Serialization.JsonPropertyName("carNumber")]
        public string? CarNumber { get; set; }

        [MaxLength(200)]
        [System.Text.Json.Serialization.JsonPropertyName("transporterName")]
        public string? TransporterName { get; set; }

        [MaxLength(50)]
        [System.Text.Json.Serialization.JsonPropertyName("transporterCode")]
        public string? TransporterCode { get; set; }

        [MaxLength(50)]
        [System.Text.Json.Serialization.JsonPropertyName("iqamaNumber")]
        public string? IqamaNumber { get; set; }

        [MaxLength(200)]
        [System.Text.Json.Serialization.JsonPropertyName("driverName")]
        public string? DriverName { get; set; }

        [MaxLength(100)]
        [System.Text.Json.Serialization.JsonPropertyName("area")]
        public string? Area { get; set; }
    }
}

