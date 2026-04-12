using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Commands.Support.Create;
using Application.Commands.Support.AddMessage;
using Application.Queries.Support;
using Application.DTOs.Support;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Dealer")]
    public class DealerSupportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DealerSupportController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Get support contact information (static data to match frontend)
        /// </summary>
        [HttpGet("contact-info")]
        public IActionResult GetContactInfo()
        {
            var contactInfo = new
            {
                supportPhone = "+91-124-4567890",
                supportEmail = "support@ucicautoparts.com",
                emergencyPhone = "+91-98765-43210",
                businessHours = "Mon-Fri: 9:00 AM - 6:00 PM",
                whatsAppNumber = "+91-98765-43210"
            };
            return Ok(contactInfo);
        }

        /// <summary>
        /// Get FAQs (static data to match frontend)
        /// </summary>
        [HttpGet("faqs")]
        public IActionResult GetFAQs()
        {
            var faqs = new[]
            {
                new { 
                    question = "How can I track my order?",
                    answer = "You can track your order by logging into your account and visiting the Order History section. Click on the \"Track\" button next to your order to see real-time updates.",
                    category = "Orders"
                },
                new { 
                    question = "What is your return policy?",
                    answer = "We accept returns within 30 days of delivery for unused items in original packaging. Please contact our support team to initiate a return.",
                    category = "Orders"
                },
                new { 
                    question = "How do I know if a part is compatible with my vehicle?",
                    answer = "Each product page includes a compatibility checker. Enter your vehicle details (make, model, year) to verify compatibility. You can also contact our technical team for assistance.",
                    category = "Products"
                },
                new { 
                    question = "What are your shipping charges?",
                    answer = "Shipping charges vary based on order value and location. Orders above ₹5000 qualify for free shipping. You can view exact charges during checkout.",
                    category = "Shipping"
                },
                new { 
                    question = "How do I update my company profile?",
                    answer = "Go to the Company Profile section in your account dashboard. You can update all company information, addresses, and documents there.",
                    category = "Account"
                }
            };
            return Ok(faqs);
        }

        /// <summary>
        /// Submit a new support ticket
        /// </summary>
        [HttpPost("tickets")]
        public async Task<IActionResult> CreateSupportTicket([FromBody] CreateSupportTicketDTO ticketDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateSupportTicketCommand
            {
                Subject = ticketDto.Subject,
                Description = ticketDto.Description,
                Category = ticketDto.Category,
                Priority = ticketDto.Priority
            };

            var result = await _mediator.Send(command);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Get support tickets for the current dealer
        /// </summary>
        [HttpGet("tickets")]
        public async Task<IActionResult> GetSupportTickets(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetDealerSupportTicketsQuery
            {
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        /// <summary>
        /// Get a specific support ticket by ID
        /// </summary>
        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetSupportTicketById(int id)
        {
            var query = new GetSupportTicketByIdQuery { TicketId = id };
            var result = await _mediator.Send(query);
            
            if (result.Success)
                return Ok(result);
            else
                return NotFound(result);
        }

        /// <summary>
        /// Add a message to an existing support ticket
        /// </summary>
        [HttpPost("tickets/{id}/messages")]
        public async Task<IActionResult> AddMessageToTicket(int id, [FromBody] AddMessageToTicketDTO messageDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new AddMessageToTicketCommand
            {
                TicketId = id,
                Message = messageDto.Message
            };

            var result = await _mediator.Send(command);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}