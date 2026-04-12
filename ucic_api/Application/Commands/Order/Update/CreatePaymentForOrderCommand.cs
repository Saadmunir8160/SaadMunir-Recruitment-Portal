using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Repositories.Command.Base;
using Domain.Repositories.Query.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace Application.Commands.Order.Update
{
    public class CreatePaymentForOrderCommand : IRequest<bool>
    {
        [Required(ErrorMessage = "Order ID is required")]
        public long OrderId { get; set; }
        [Required(ErrorMessage = "Transaction ID is required")]
        public string TransactionId { get; set; }
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Payment File  is required")]
        public IFormFile PaymentFile { get; set; }
    }

    public class CreatePaymentForOrderCommandHandler : IRequestHandler<CreatePaymentForOrderCommand, bool>
    {
        private readonly ICommandRepository<Domain.Entities.Order> _commandRepository;
        private readonly IQueryRepository<Domain.Entities.Order> _queryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageFileService _FileService;

        public CreatePaymentForOrderCommandHandler(
            ICommandRepository<Domain.Entities.Order> commandRepository,
            IQueryRepository<Domain.Entities.Order> queryRepository,
            IHttpContextAccessor httpContextAccessor,
            IImageFileService imageFileService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _httpContextAccessor = httpContextAccessor;
            _FileService = imageFileService;
        }

        public async Task<bool> Handle(CreatePaymentForOrderCommand request, CancellationToken cancellationToken)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var existingOrder = await _queryRepository.GetByIdAsync(request.OrderId);
            if (existingOrder == null)
            {
                return false;
            }

            var allowedExtensions = new[] { ".pdf", ".png", ".jpeg", ".jpg", ".gif" };
            long maxFileSize = 5 * 1024 * 1024; // 5 MB in bytes
            string? orderPaymentFilePath = null;


            if (request.PaymentFile != null)
            {
                // Verify file type
                var extension = Path.GetExtension(request.PaymentFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return false;
                }

                // Verify file size
                if (request.PaymentFile.Length > maxFileSize)
                {
                    return false;
                }

                // Upload File to path
                orderPaymentFilePath = await _FileService.SaveFileAsync(request.PaymentFile, "Payment");
            }
            existingOrder.PaymentSubmittedDate = DateTime.Now;
            existingOrder.PaymentFilePath = orderPaymentFilePath;
            existingOrder.PaymentRemarks = request.Remarks;
            existingOrder.PaymentTransactionId = request.TransactionId;
            existingOrder.Status = OrderStatus.PaymentSubmitted;
            existingOrder.PaymentAmount = existingOrder.TotalPrice;

            await _commandRepository.UpdateAsync(existingOrder); // UpdateAsync returns void, so no assignment is needed.

            return true;
        }

    }
}