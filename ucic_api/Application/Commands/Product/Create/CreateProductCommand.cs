using Domain.Repositories.Command.Base;
using Microsoft.AspNetCore.Http;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Common.Interfaces;

namespace Application.Commands.Product.Create
{
    public class CreateProductCommand : IRequest<Response<CreateProductCommand>>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "CoverageAreaId is required")]
        public long CoverageAreaId { get; set; }

        [Required(ErrorMessage = "ImageUrl is required")]
        public IFormFile? ProductFilePath { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "VatPercentage is required")]
        public decimal VatPercentage { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(30, ErrorMessage = "Sku cannot exceed 100 characters")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "ProductCode is required")]
        [StringLength(100, ErrorMessage = "ProductCode cannot exceed 100 characters")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(30, ErrorMessage = "Type cannot exceed 100 characters")]
        public string Type { get; set; }
        public string? ArabicName { get; set; }
        public string? ArabicDescription { get; set; }

        [Required(ErrorMessage = "DiscountPercentage is required")]
        public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "ShipingCostPercentage is required")]
        public decimal ShipingCostPercentage { get; set; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Response<CreateProductCommand>>
    {
        private readonly ICommandRepository<Domain.Entities.Product> _commandRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        private readonly IImageFileService _FileService;
        public CreateProductCommandHandler(ICommandRepository<Domain.Entities.Product> commandRepository, IHttpContextAccessor httpContextAccessor, ILogger<CreateProductCommandHandler> logger, IImageFileService fileService)
        {
            _commandRepository = commandRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
           _FileService = fileService;
        }
        public async Task<Response<CreateProductCommand>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            string? ProductImageUrl = null;
            var allowedExtensions = new[] { ".pdf", ".png", ".jpeg", ".jpg", ".gif" };
            long maxFileSize = 5 * 1024 * 1024; // 5 MB in bytes
            if (request.ProductFilePath != null)
            {
                // Verify file type
                var extension = Path.GetExtension(request.ProductFilePath.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return new Response<CreateProductCommand>
                    {
                        Success = false,
                        Message = "\"Invalid file type. Only .pdf .png, .jpeg, .jpg, and .gif are allowed.",
                        Data = request
                    };
                }

                // Verify file size
                if (request.ProductFilePath.Length > maxFileSize)
                {
                    return new Response<CreateProductCommand>
                    {
                        Success = false,
                        Message = "File size exceeds the limit of 5 MB.",
                        Data = request
                    };
                } 

                // Upload File to path
                ProductImageUrl = await _FileService.SaveFileAsync(request.ProductFilePath, "Product");
                ProductImageUrl = Path.GetFileName(ProductImageUrl);
            }
            var username = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            var item = new Domain.Entities.Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                VatPercentage = request.VatPercentage,
                CoverageAreaId = request.CoverageAreaId,
                ImageUrl = ProductImageUrl,
                Sku = request.Sku,
                Code = request.ProductCode,
                Type = request.Type,
                DiscountPercentage = request.DiscountPercentage,
                ShippingCostPercentage = request.ShipingCostPercentage,
                ArabicName = request.ArabicName,
                ArabicDescription = request.ArabicDescription,
                CreatedDate = DateTime.Now,
                CreatedBy = username.IsNullOrEmpty() ? null : username,
            };
            var result = await _commandRepository.AddAsync(item);

            if (result > 0)
            {
                return new Response<CreateProductCommand>
                {
                    Success = true,
                    Message = "Product created successfully.",
                    Data = request
                };
            }

            return new Response<CreateProductCommand>
            {
                Success = false,
                Message = "Failed to create the product.",
                Data = request
            };
        }
    }
}