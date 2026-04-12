using Domain.Repositories.Command.Base;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.DTOs.Support;

namespace Application.Commands.Support.Create
{
    public class CreateSupportTicketCommand : IRequest<Response<SupportTicketDTO>>
    {
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
        public string Priority { get; set; }

        public long? OrderId { get; set; }
        public long? ProductId { get; set; }
        // Note: DealerId is automatically retrieved from current user context
    }
}