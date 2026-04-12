using Application.Queries.Admin.DealerProduct;
using Application.Queries.Admin.DealerOrder;
using Application.Queries.Admin.DealerDriver;
using Application.Queries.Admin.DealerVehicle;
using Application.Queries.Admin.DealerManagement;
using Application.Queries.Admin.DailyLimits;
using Application.Queries.Admin.DealerAddress;
using Application.Queries.Admin.DealerArea;
using Application.Queries.Admin.SupportTicket;
using Application.Commands.Admin.DailyLimits;
using Application.Commands.Admin.DealerAddress;
using Application.Commands.Admin.DealerArea;
using Application.Commands.Admin.SupportTicket;
using Application.Commands.Admin.DealerProduct;
using Application.Commands.Admin.DealerDriver;
using Application.Commands.Admin.DealerVehicle;
using Application.Commands.DealerEntities;
using DeleteVehicleCommand = Application.Commands.DealerVehicle.DeleteDealerVehicleCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/Admin/DealerManagement")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDealerManagementController : Controller
    {
        private readonly IMediator _mediator;

        public AdminDealerManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Dealer Products Admin Endpoints
        [HttpGet("Products")]
        public async Task<ActionResult> GetAllDealerProducts(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] bool? isActive = null)
        {
            var query = new GetAllDealerProductsForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                IsActive = isActive
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Products/{id}")]
        public async Task<ActionResult> GetDealerProductById(int id)
        {
            var query = new GetDealerProductByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Products/Stats")]
        public async Task<ActionResult> GetDealerProductsStats([FromQuery] int? dealerId = null)
        {
            var query = new GetDealerProductsStatsForAdminQuery { DealerId = dealerId };
            return Ok(await _mediator.Send(query));
        }

        [HttpPut("Products/Update/{id}")]
        public async Task<ActionResult> UpdateDealerProduct(int id, [FromBody] UpdateDealerProductForAdminCommand command)
        {
            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Products/{id}")]
        public async Task<ActionResult> DeleteDealerProduct(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerProductForAdminCommand { Id = id }));
        }

        // Dealer Orders Admin Endpoints
        [HttpGet("Orders")]
        public async Task<ActionResult> GetAllDealerOrders(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = new GetAllDealerOrdersForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                Status = status,
                StartDate = startDate,
                EndDate = endDate
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Orders/{id}")]
        public async Task<ActionResult> GetDealerOrderById(int id)
        {
            var query = new GetDealerOrderByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Orders/Stats")]
        public async Task<ActionResult> GetDealerOrdersStats([FromQuery] int? dealerId = null)
        {
            var query = new GetDealerOrdersStatsForAdminQuery { DealerId = dealerId };
            return Ok(await _mediator.Send(query));
        }

        // Dealer Drivers Admin Endpoints
        [HttpGet("Drivers")]
        public async Task<ActionResult> GetAllDealerDrivers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] bool? isActive = null)
        {
            var query = new GetAllDealerDriversForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                IsActive = isActive
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Drivers/{id}")]
        public async Task<ActionResult> GetDealerDriverById(int id)
        {
            var query = new GetDealerDriverByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Drivers/Stats")]
        public async Task<ActionResult> GetDealerDriversStats([FromQuery] int? dealerId = null)
        {
            var query = new GetDealerDriversStatsForAdminQuery { DealerId = dealerId };
            return Ok(await _mediator.Send(query));
        }

        [HttpDelete("Drivers/{id}")]
        public async Task<ActionResult> DeleteDealerDriver(int id)
        {
            return Ok(await _mediator.Send(new DeleteDealerDriverCommand { DriverID = id }));
        }

        [HttpPut("Drivers/{id}/Activate")]
        public async Task<ActionResult> ActivateDealerDriver(int id)
        {
            return Ok(await _mediator.Send(new ActivateDealerDriverCommand { DriverID = id }));
        }

        [HttpPut("Drivers/{id}/Deactivate")]
        public async Task<ActionResult> DeactivateDealerDriver(int id)
        {
            return Ok(await _mediator.Send(new DeactivateDealerDriverCommand { DriverID = id }));
        }

        [HttpPut("Drivers/Update/{id}")]
        public async Task<ActionResult> UpdateDealerDriver(int id, [FromBody] UpdateDealerDriverForAdminCommand command)
        {
            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        // Dealer Vehicles Admin Endpoints
        [HttpGet("Vehicles")]
        public async Task<ActionResult> GetAllDealerVehicles(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? vehicleType = null)
        {
            var query = new GetAllDealerVehiclesForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                IsActive = isActive,
                VehicleType = vehicleType
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Vehicles/{id}")]
        public async Task<ActionResult> GetDealerVehicleById(int id)
        {
            var query = new GetDealerVehicleByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Vehicles/Stats")]
        public async Task<ActionResult> GetDealerVehiclesStats([FromQuery] int? dealerId = null)
        {
            var query = new GetDealerVehiclesStatsForAdminQuery { DealerId = dealerId };
            return Ok(await _mediator.Send(query));
        }

        [HttpDelete("Vehicles/{id}")]
        public async Task<ActionResult> DeleteDealerVehicle(int id)
        {
            return Ok(await _mediator.Send(new DeleteVehicleCommand { VehicleID = id }));
        }

        [HttpPut("Vehicles/{id}/Activate")]
        public async Task<ActionResult> ActivateDealerVehicle(int id)
        {
            return Ok(await _mediator.Send(new Application.Commands.DealerEntities.ActivateDealerVehicleCommand { VehicleID = id }));
        }

        [HttpPut("Vehicles/{id}/Deactivate")]
        public async Task<ActionResult> DeactivateDealerVehicle(int id)
        {
            return Ok(await _mediator.Send(new Application.Commands.DealerEntities.DeactivateDealerVehicleCommand { VehicleID = id }));
        }

        [HttpPut("Vehicles/Update/{id}")]
        public async Task<ActionResult> UpdateDealerVehicle(int id, [FromBody] UpdateDealerVehicleForAdminCommand command)
        {
            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        // General Admin Endpoints
        [HttpGet("Dashboard/Stats")]
        public async Task<ActionResult> GetDashboardStats()
        {
            var query = new GetDealerManagementDashboardStatsQuery();
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Dealers/Summary")]
        public async Task<ActionResult> GetDealersSummary()
        {
            var query = new GetDealersSummaryForAdminQuery();
            return Ok(await _mediator.Send(query));
        }

        // Dealer Daily Limits Admin Endpoints
        [HttpGet("DailyLimits")]
        public async Task<ActionResult> GetAllDealerDailyLimits(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] string? limitType = null)
        {
            var query = new GetAllDealerDailyLimitsForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                LimitType = limitType
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("DailyLimits/{id}")]
        public async Task<ActionResult> GetDealerDailyLimitById(int id)
        {
            var query = new GetDealerDailyLimitByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("DailyLimits/Create")]
        public async Task<ActionResult> CreateDealerDailyLimit([FromBody] CreateDealerDailyLimitCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("DailyLimits/Update/{id}")]
        public async Task<ActionResult> UpdateDealerDailyLimit(int id, [FromBody] UpdateDealerDailyLimitCommand command)
        {
            command.DailyLimitID = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("DailyLimits/Delete/{id}")]
        public async Task<ActionResult> DeleteDealerDailyLimit(int id)
        {
            var command = new DeleteDealerDailyLimitCommand { DailyLimitID = id };
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("DailyLimits/Stats")]
        public async Task<ActionResult> GetDealerDailyLimitsStats([FromQuery] int? dealerId = null)
        {
            var query = new GetDealerDailyLimitsStatsForAdminQuery { DealerId = dealerId };
            return Ok(await _mediator.Send(query));
        }

        // Dealer Addresses Admin Endpoints
        [HttpGet("Addresses")]
        public async Task<ActionResult> GetAllDealerAddresses(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] string? addressType = null)
        {
            var query = new GetAllDealerAddressesForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                AddressType = addressType
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Addresses/{id}")]
        public async Task<ActionResult> GetDealerAddressById(int id)
        {
            var query = new GetDealerAddressByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("Addresses/Create")]
        public async Task<ActionResult> CreateDealerAddress([FromBody] CreateDealerAddressCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("Addresses/Update/{id}")]
        public async Task<ActionResult> UpdateDealerAddress(int id, [FromBody] UpdateDealerAddressCommand command)
        {
            command.AddressId = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Addresses/Delete/{id}")]
        public async Task<ActionResult> DeleteDealerAddress(int id)
        {
            var command = new DeleteDealerAddressCommand { AddressId = id };
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("Addresses/SetDefault")]
        public async Task<ActionResult> SetDefaultDealerAddress([FromBody] SetDefaultDealerAddressCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        // Dealer Areas Admin Endpoints
        [HttpGet("Areas")]
        public async Task<ActionResult> GetAllDealerAreas(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null)
        {
            var query = new GetAllDealerAreasForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                IsActive = isActive
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("Areas/{id}")]
        public async Task<ActionResult> GetDealerAreaById(int id)
        {
            var query = new GetDealerAreaByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("Areas/Create")]
        public async Task<ActionResult> CreateDealerArea([FromBody] CreateDealerAreaForAdminCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("Areas/Update/{id}")]
        public async Task<ActionResult> UpdateDealerArea(int id, [FromBody] UpdateDealerAreaForAdminCommand command)
        {
            command.Id = id;
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("Areas/Delete/{id}")]
        public async Task<ActionResult> DeleteDealerArea(int id)
        {
            var command = new DeleteDealerAreaForAdminCommand { Id = id };
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("Areas/Stats")]
        public async Task<ActionResult> GetDealerAreasStats()
        {
            var query = new GetDealerAreasStatsForAdminQuery();
            return Ok(await _mediator.Send(query));
        }

        // Support Tickets Admin Endpoints
        [HttpGet("SupportTickets")]
        public async Task<ActionResult> GetAllSupportTickets(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? search = null,
            [FromQuery] int? dealerId = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null)
        {
            var query = new GetAllSupportTicketsForAdminQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                DealerId = dealerId,
                Category = category,
                Status = status,
                Priority = priority
            };
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("SupportTickets/{id}")]
        public async Task<ActionResult> GetSupportTicketById(int id)
        {
            var query = new GetSupportTicketByIdForAdminQuery { Id = id };
            return Ok(await _mediator.Send(query));
        }

        [HttpPost("SupportTickets/AddResponse")]
        public async Task<ActionResult> AddTicketResponse([FromBody] AddTicketResponseCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("SupportTickets/UpdateStatus")]
        public async Task<ActionResult> UpdateTicketStatus([FromBody] UpdateTicketStatusCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("SupportTickets/Assign")]
        public async Task<ActionResult> AssignTicket([FromBody] AssignTicketCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("SupportTickets/UpdatePriority")]
        public async Task<ActionResult> UpdateTicketPriority([FromBody] UpdateTicketPriorityCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("SupportTickets/Export")]
        public async Task<ActionResult> ExportSupportTickets(
            [FromQuery] int? dealerId = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? search = null)
        {
            var query = new ExportSupportTicketsForAdminQuery
            {
                DealerId = dealerId,
                Category = category,
                Status = status,
                Priority = priority,
                Search = search
            };
            var result = await _mediator.Send(query);
            return File(result.FileContent, result.ContentType, result.FileName);
        }
    }
}